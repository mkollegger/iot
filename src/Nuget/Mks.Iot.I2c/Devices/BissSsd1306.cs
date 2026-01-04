#region License

// #region License
// MIT License
// 
// Copyright (C) 2026 Michael Kollegger
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// #endregion

#endregion

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using Biss.Extensions;
using Biss.Log.Producer;
using Iot.Device.Graphics;
using Iot.Device.Graphics.SkiaSharpAdapter;
using Iot.Device.Ssd13xx;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using SkiaSharp;

namespace Mks.Iot.I2c.Devices;

#region Enums

/// <summary>
///     Line Modes für BissSsd1306
/// </summary>
public enum EnumBissSsd1306LineModes
{
    /// <summary>
    ///     4 Zeilen, 25 Zeichen pro Zeile
    /// </summary>
    LineMode4,

    /// <summary>
    ///     2 Zeilen, 13 Zeichen pro Zeile
    /// </summary>
    LineMode2,

    /// <summary>
    ///     1 Zeile, 13 Zeichen pro Zeile
    /// </summary>
    LineMode1
}

#endregion

/// <summary>
///     <para>Ssd1306 - BISS Impementierung</para>
///     Klasse BissSsd1306.
///     https://github.com/dotnet/iot/tree/main/src/devices/Ssd13xx
/// </summary>
public class BissSsd1306 : Ssd1306
{
    private readonly string _fontFamiliyName;
    private readonly Lock _writeTextLock = new Lock();
    private EnumBissSsd1306LineModes _lineMode;
    private int _maxCharPerLine;
    private SKFont _skFont;
    private SKPaint _skPaint;


    /// <summary>
    ///     Ssd1306 - BISS Impementierung
    /// </summary>
    /// <param name="i2CDevice">I²C Device</param>
    /// <param name="lineModes">Modi für Textausgabe</param>
    /// <param name="fontFamiliyName">Font - Default ist "Courier New"</param>
    public BissSsd1306(I2cDevice i2CDevice,
        EnumBissSsd1306LineModes lineModes = EnumBissSsd1306LineModes.LineMode2,
        string fontFamiliyName = "Courier New") : base(i2CDevice, 128, 32)
    {
        ArgumentNullException.ThrowIfNull(i2CDevice);

        SkiaSharpAdapter.Register();
        LineMode = lineModes;

        ArgumentNullException.ThrowIfNull(_skFont);
        ArgumentNullException.ThrowIfNull(_skPaint);
        _fontFamiliyName = fontFamiliyName;
    }

    #region Properties

    /// <summary>
    ///     Gets or sets the line mode for the display, which determines the number of characters per line and the font
    ///     size.
    /// </summary>
    /// <remarks>
    ///     Changing the line mode will adjust the maximum number of characters per line and the
    ///     font size used for rendering text.
    /// </remarks>
    public EnumBissSsd1306LineModes LineMode
    {
        get { return _lineMode; }
        set
        {
            if (_lineMode == value && _skPaint != null! && _skFont != null!)
            {
                return;
            }

            _lineMode = value;

            int fontSize;
            switch (_lineMode)
            {
                case EnumBissSsd1306LineModes.LineMode4:
                    _maxCharPerLine = 25;
                    fontSize = 8;
                    break;
                case EnumBissSsd1306LineModes.LineMode2:
                    _maxCharPerLine = 13;
                    fontSize = 16;
                    break;
                case EnumBissSsd1306LineModes.LineMode1:
                    _maxCharPerLine = 8;
                    fontSize = 32;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _skFont = new SKFont(SKTypeface.FromFamilyName(_fontFamiliyName), fontSize);
            _skPaint = new SKPaint(_skFont)
            {
                Color = SKColors.White,
                TextEncoding = SKTextEncoding.Utf16,
                IsAntialias = false,
                IsLinearText = true,
                IsStroke = false
            };

            ClearScreen();
        }
    }

    #endregion

    /// <summary>
    ///     Textausgabe auf dem Display
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="row">Zeile</param>
    /// <param name="textAlign">Textausrichtung</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void WriteText(string text, int row, SKTextAlign textAlign = SKTextAlign.Left)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        lock (_writeTextLock)
        {
            text = text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (text.Length > _maxCharPerLine)
            {
                Logging.Log.TryLogWarning($"[{GetType().Name}]({nameof(WriteText)}): Text is too long for the display. Truncating to {_maxCharPerLine} characters.");
                text = text.Substring(0, _maxCharPerLine);
            }

            switch (_lineMode)
            {
                case EnumBissSsd1306LineModes.LineMode4:
                    if (row < 0 || row > 3)
                    {
                        throw new ArgumentOutOfRangeException(nameof(row), "Row number must be between 0 and 3.");
                    }

                    SendCommand(new SetPageAddress((PageAddress) row));
                    break;
                case EnumBissSsd1306LineModes.LineMode2:
                    if (row < 0 || row > 1)
                    {
                        throw new ArgumentOutOfRangeException(nameof(row), "Row number must be between 0 and 1.");
                    }

                    SendCommand(new SetPageAddress((PageAddress) (row * 2)));
                    break;
                case EnumBissSsd1306LineModes.LineMode1:
                    if (row != 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(row), "Row number must be 0 for single line mode.");
                    }

                    SendCommand(new SetPageAddress());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SendCommand(new SetColumnAddress());

            int size = (int) _skFont.Size;
            using BitmapImage image = BitmapImage.CreateBitmap(128, size, PixelFormat.Format32bppArgb);
            image.Clear(Color.Black);

            IGraphics i = image.GetDrawingApi();
            SKCanvas canvas = i.GetCanvas();

            int x = 0;

            if (textAlign != SKTextAlign.Left)
            {
                int textWidth = Convert.ToInt32(Math.Round(_skPaint.MeasureText(text), 0));
                switch (textAlign)
                {
                    case SKTextAlign.Center:
                        x = (128 - textWidth) / 2;
                        break;
                    case SKTextAlign.Right:
                        x = 128 - textWidth;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(textAlign), "Unsupported text alignment.");
                }
            }

            canvas.DrawText(text, x, size, _skPaint);

            byte[] bytes = PreRenderBitmap(image);
            SendData(bytes);
        }
    }

    /// <summary>
    ///     Zeile löschen
    /// </summary>
    /// <param name="lineNumber"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void ClearLine(int lineNumber)
    {
        if (lineNumber < 0 || lineNumber > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(lineNumber), "Line number must be between 0 and 3.");
        }

        byte[] data = new byte[128];
        SendCommand(new SetColumnAddress());
        SendCommand(new SetPageAddress((PageAddress) lineNumber));
        SendData(data);
    }

    /// <inheritdoc />
    public override void ClearScreen()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        byte[] data = new byte[128];
        for (int i = 0; i < 4; i++)
        {
            SendCommand(new SetColumnAddress());
            SendCommand(new SetPageAddress((PageAddress) i));
            SendData(data);
        }

        sw.Stop();
        Logging.Log.TryLogTrace($"[{GetType().Name}]({nameof(ClearScreen)}): ClearScreen took {sw.ElapsedMilliseconds} ms");
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _skFont.Dispose();
            _skPaint.Dispose();
        }
    }
}