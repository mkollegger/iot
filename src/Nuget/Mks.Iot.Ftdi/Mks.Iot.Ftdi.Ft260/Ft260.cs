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
using System.Collections.Generic;
using System.Device.I2c;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Pcx857x;
using Iot.Device.Ssd13xx;
using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using Microsoft.Extensions.Logging;

namespace Mks.Iot.Ftdi.Ft260;
//public class MySsd1306Bitmap : BitmapImage
//{
//    // Datenpuffer für das Bild
//    private readonly byte[] _buffer;

//    public MySsd1306Bitmap(int width, int height, int stride, PixelFormat pixelFormat)
//        : base(width, height, stride, pixelFormat)
//    {
//        _buffer = new byte[stride * height];
//    }

//    // Zugriff auf die Rohdaten (für SSD1306 wichtig)
//    public override Span<byte> AsByteSpan()
//    {
//        return new Span<byte>(_buffer);
//    }

//    /// <inheritdoc />
//    public override void SaveToStream(Stream stream, ImageFileType format)
//    {
//        throw new NotImplementedException();
//    }

//    /// <inheritdoc />
//    protected override void Dispose(bool disposing)
//    {
//        if (disposing)
//            return;
//        base.Dispose();
//    }

//    /// <inheritdoc />
//    public override IGraphics GetDrawingApi()
//    {
//        throw new NotImplementedException();
//    }

//    // Pixel setzen (vereinfacht, für 1bpp)
//    public override void SetPixel(int x, int y, Color color)
//    {
//        int byteIndex = (y * Stride) + (x / 8);
//        int bitIndex = x % 8;

//        if (color.GetBrightness() > 0.5)
//            _buffer[byteIndex] |= (byte)(1 << bitIndex); // Weiß
//        else
//            _buffer[byteIndex] &= (byte)~(1 << bitIndex); // Schwarz
//    }

//    // Pixel lesen (vereinfacht, für 1bpp)
//    public override Color GetPixel(int x, int y)
//    {
//        int byteIndex = (y * Stride) + (x / 8);
//        int bitIndex = x % 8;
//        bool isSet = (_buffer[byteIndex] & (1 << bitIndex)) != 0;
//        return isSet ? Color.White : Color.Black;
//    }
//}

//public class MySsd1306ImageFactory : IImageFactory
//{
//    public BitmapImage CreateBitmap(int width, int height, PixelFormat pixelFormat)
//    {
//        // Für SSD1306 ist Stride = (width + 7) / 8 (bei 1bpp)
//        int stride = (width + 7) / 8;
//        return new MySsd1306Bitmap(width, height, stride, pixelFormat);
//    }

//    public BitmapImage CreateFromFile(string filename)
//    {
//        // Optional: Bild aus Datei laden, hier nur als Beispiel
//        throw new NotImplementedException("Not implemented for this example.");
//    }

//    public BitmapImage CreateFromStream(Stream stream)
//    {
//        // Optional: Bild aus Stream laden, hier nur als Beispiel
//        throw new NotImplementedException("Not implemented for this example.");
//    }
//}

#region BasicFont

//https://github.com/dotnet/iot/blob/main/src/devices/Ssd13xx/samples/BasicFont.cs
internal class BasicFont
{
    #region Properties

    private static IDictionary<char, byte[]> FontCharacterData =>
        new Dictionary<char, byte[]>
        {
            // Special Characters.
            {' ', new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00}},
            {'!', new byte[] {0x00, 0x00, 0x5F, 0x00, 0x00, 0x00}},
            {'\"', new byte[] {0x00, 0x06, 0x00, 0x06, 0x00, 0x00}},
            {'#', new byte[] {0x14, 0x7F, 0x14, 0x7F, 0x14, 0x00}},
            {'$', new byte[] {0x04, 0x2A, 0x6D, 0x2A, 0x10, 0x00}},
            {'%', new byte[] {0x27, 0x16, 0x08, 0x34, 0x32, 0x00}},
            {'&', new byte[] {0x20, 0x56, 0x49, 0x36, 0x50, 0x00}},
            {'\'', new byte[] {0x00, 0x03, 0x05, 0x00, 0x00, 0x00}},
            {'(', new byte[] {0x00, 0x00, 0x1D, 0x22, 0x41, 0x00}},
            {')', new byte[] {0x41, 0x22, 0x1D, 0x00, 0x00, 0x00}},
            {'*', new byte[] {0x14, 0x08, 0x3E, 0x08, 0x14, 0x00}},
            {'+', new byte[] {0x20, 0x56, 0x49, 0x36, 0x50, 0x00}},
            {',', new byte[] {0x00, 0x50, 0x30, 0x00, 0x00, 0x00}},
            {'-', new byte[] {0x08, 0x08, 0x08, 0x08, 0x08, 0x00}},
            {'.', new byte[] {0x00, 0x30, 0x30, 0x00, 0x00, 0x00}},
            {'/', new byte[] {0x20, 0x10, 0x08, 0x04, 0x02, 0x00}},
            {':', new byte[] {0x00, 0x36, 0x36, 0x00, 0x00, 0x00}},
            {';', new byte[] {0x00, 0x56, 0x36, 0x00, 0x00, 0x00}},
            {'<', new byte[] {0x08, 0x14, 0x22, 0x41, 0x00, 0x00}},
            {'=', new byte[] {0x14, 0x14, 0x14, 0x14, 0x14, 0x00}},
            {'>', new byte[] {0x00, 0x41, 0x22, 0x14, 0x08, 0x00}},
            {'?', new byte[] {0x02, 0x01, 0x51, 0x09, 0x06, 0x00}},
            {'@', new byte[] {0x3E, 0x41, 0x4D, 0x4D, 0x06, 0x00}},
            {'[', new byte[] {0x00, 0x7F, 0x41, 0x41, 0x00}},
            {'\\', new byte[] {0x02, 0x04, 0x08, 0x10, 0x20}},
            {']', new byte[] {0x00, 0x41, 0x41, 0x7F, 0x00}},
            {'^', new byte[] {0x04, 0x02, 0x01, 0x02, 0x04}},
            {'`', new byte[] {0x00, 0x00, 0x05, 0x03, 0x00}},

            // Numbers.
            {'0', new byte[] {0x3E, 0x51, 0x49, 0x45, 0x3E, 0x00}},
            {'1', new byte[] {0x00, 0x42, 0x7F, 0x40, 0x00, 0x00}},
            {'2', new byte[] {0x42, 0x61, 0x51, 0x49, 0x46, 0x00}},
            {'3', new byte[] {0x21, 0x41, 0x45, 0x4B, 0x31, 0x00}},
            {'4', new byte[] {0x18, 0x14, 0x12, 0x7F, 0x10, 0x00}},
            {'5', new byte[] {0x27, 0x45, 0x45, 0x45, 0x39, 0x00}},
            {'6', new byte[] {0x3C, 0x4A, 0x49, 0x49, 0x30, 0x00}},
            {'7', new byte[] {0x01, 0x71, 0x09, 0x05, 0x03, 0x00}},
            {'8', new byte[] {0x36, 0x49, 0x49, 0x49, 0x36, 0x00}},
            {'9', new byte[] {0x06, 0x49, 0x49, 0x29, 0x1E, 0x00}},

            // Characters.
            {'A', new byte[] {0x7E, 0x11, 0x11, 0x11, 0x7E, 0x00}},
            {'B', new byte[] {0x7F, 0x49, 0x49, 0x49, 0x36, 0x00}},
            {'C', new byte[] {0x3E, 0x41, 0x41, 0x41, 0x22, 0x00}},
            {'D', new byte[] {0x7F, 0x41, 0x41, 0x22, 0x1C, 0x00}},
            {'E', new byte[] {0x7F, 0x49, 0x49, 0x49, 0x41, 0x00}},
            {'F', new byte[] {0x7F, 0x09, 0x09, 0x09, 0x01, 0x00}},
            {'G', new byte[] {0x3E, 0x41, 0x49, 0x49, 0x7A, 0x00}},
            {'H', new byte[] {0x7F, 0x08, 0x08, 0x08, 0x7F, 0x00}},
            {'I', new byte[] {0x00, 0x41, 0x7F, 0x41, 0x00, 0x00}},
            {'J', new byte[] {0x20, 0x40, 0x41, 0x3F, 0x01, 0x00}},
            {'K', new byte[] {0x7F, 0x08, 0x14, 0x22, 0x41, 0x00}},
            {'L', new byte[] {0x7F, 0x40, 0x40, 0x40, 0x40, 0x00}},
            {'M', new byte[] {0x7F, 0x02, 0x0C, 0x02, 0x7F, 0x00}},
            {'N', new byte[] {0x7F, 0x04, 0x08, 0x10, 0x7F, 0x00}},
            {'O', new byte[] {0x3E, 0x41, 0x41, 0x41, 0x3E, 0x00}},
            {'P', new byte[] {0x7F, 0x09, 0x09, 0x09, 0x06, 0x00}},
            {'Q', new byte[] {0x3E, 0x41, 0x51, 0x21, 0x5E, 0x00}},
            {'R', new byte[] {0x7F, 0x09, 0x19, 0x29, 0x46, 0x00}},
            {'S', new byte[] {0x46, 0x49, 0x49, 0x49, 0x31, 0x00}},
            {'T', new byte[] {0x01, 0x01, 0x7F, 0x01, 0x01, 0x00}},
            {'U', new byte[] {0x3F, 0x40, 0x40, 0x40, 0x3F, 0x00}},
            {'V', new byte[] {0x1F, 0x20, 0x40, 0x20, 0x1F, 0x00}},
            {'W', new byte[] {0x3F, 0x40, 0x38, 0x40, 0x3F, 0x00}},
            {'X', new byte[] {0x63, 0x14, 0x08, 0x14, 0x63, 0x00}},
            {'Y', new byte[] {0x07, 0x08, 0x70, 0x08, 0x07, 0x00}},
            {'Z', new byte[] {0x61, 0x51, 0x49, 0x45, 0x43, 0x00}},
            // Small letters
            {'a', new byte[] {0x20, 0x54, 0x54, 0x54, 0x78}},
            {'b', new byte[] {0x7F, 0x48, 0x44, 0x44, 0x38}},
            {'c', new byte[] {0x38, 0x44, 0x44, 0x44, 0x20}},
            {'d', new byte[] {0x38, 0x44, 0x44, 0x48, 0x7F}},
            {'e', new byte[] {0x38, 0x54, 0x54, 0x54, 0x18}},
            {'f', new byte[] {0x08, 0x7E, 0x09, 0x01, 0x02}},
            {'g', new byte[] {0x04, 0x2A, 0x2A, 0x2A, 0x1C}},
            {'h', new byte[] {0x7F, 0x08, 0x04, 0x04, 0x78}},
            {'i', new byte[] {0x00, 0x44, 0x7D, 0x40, 0x00}},
            {'j', new byte[] {0x20, 0x40, 0x44, 0x3D, 0x00}},
            {'k', new byte[] {0x7F, 0x10, 0x28, 0x44, 0x00}},
            {'l', new byte[] {0x00, 0x41, 0x7F, 0x40, 0x00}},
            {'m', new byte[] {0x7C, 0x04, 0x18, 0x04, 0x78}},
            {'n', new byte[] {0x7C, 0x08, 0x04, 0x04, 0x78}},
            {'o', new byte[] {0x38, 0x44, 0x44, 0x44, 0x38}},
            {'p', new byte[] {0x7C, 0x14, 0x14, 0x14, 0x08}},
            {'q', new byte[] {0x08, 0x14, 0x14, 0x18, 0x7C}},
            {'r', new byte[] {0x7C, 0x08, 0x04, 0x04, 0x08}},
            {'s', new byte[] {0x48, 0x54, 0x54, 0x54, 0x20}},
            {'t', new byte[] {0x04, 0x3F, 0x44, 0x40, 0x20}},
            {'u', new byte[] {0x3C, 0x40, 0x40, 0x20, 0x7C}},
            {'v', new byte[] {0x1C, 0x20, 0x40, 0x20, 0x1C}},
            {'w', new byte[] {0x3C, 0x40, 0x30, 0x40, 0x3C}},
            {'x', new byte[] {0x44, 0x28, 0x10, 0x28, 0x44}},
            {'y', new byte[] {0x0C, 0x50, 0x50, 0x50, 0x3C}},
            {'z', new byte[] {0x44, 0x64, 0x54, 0x4C, 0x44}}
        };

    #endregion

    public static byte[] GetCharacterBytes(char character)
    {
        try
        {
            return FontCharacterData[character];
        }
        catch (Exception e)
        {
            return new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
        }
    }
}

#endregion

/// <summary>
///     <para>Dotnet für den FTDI Chip FT260 - HID-class USB to UART/I2C Bridge IC</para>
///     Klasse Ft260. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public class Ft260 : IDisposable
{
    private static byte _sampleCounter;
    private static bool _sampleBoolean;
    /// <summary>
    /// List of I2C slave addresses.
    /// </summary>
    private readonly bool _backgroundWorking = false;
    private readonly CancellationTokenSource _ctsClose = new CancellationTokenSource();
    private bool _disposedValue;
    private Pcf8574? _pcf8574;

    private bool _sampleRunning;
    private Ssd1306? _ssd1306;

    /// <summary>
    ///     Dotnet für den FTDI Chip FT260 - HID-class USB to UART/I2C Bridge IC
    /// </summary>
    public Ft260()
    {
#pragma warning disable CA2000 // Dispose objects before losing scope
        Create();
#pragma warning restore CA2000 // Dispose objects before losing scope

        //StartBackgroundWorker();
    }

    #region Properties

    /// <summary>
    ///     Alle nativen Lib Funktionen aufbereitet
    /// </summary>
    public static Ft260Wraper Ft260Base { get; private set; } = null!;

    #endregion

    /// <summary>
    ///     Sigelton der nativen Bibliothek
    /// </summary>
    /// <returns></returns>
    public static Ft260Wraper Create()
    {
        if (Ft260Base == null!)
        {
            Ft260Base = new Ft260Wraper();
            Ft260Base.I2CMaster_Init();
            Ft260Base.GetI2cDevices(true);
        }
        return Ft260Base;
    }

    /// <summary>
    ///     Sample starten oder stoppen
    /// </summary>
    /// <param name="start"></param>
    public void StartStopSample(bool start = true)
    {
        if (start == _sampleRunning)
        {
            return;
        }

        if (start)
        {
            Ft260Base.Gpio_SetDir(FT260_GPIO.FT260_GPIO_2, Ft260GpioDir.Output);
            FT260_GPIO_Report? r = Ft260Base.Gpio_Get();


#pragma warning disable CA2000 // Dispose objects before losing scope
            if (Ft260Base.I2cDevices.Contains(0x20))
            {
                _pcf8574 = new Pcf8574(I2CDeviceFt260.Create(new I2cConnectionSettings(1, 0x20)));
            }

            if (Ft260Base.I2cDevices.Contains(0x3C))
            {
                //BitmapImage.RegisterImageFactory(new MySsd1306ImageFactory());
                //var x = BitmapImage.CreateBitmap(128, 64, PixelFormat.Format1bppBw);


                //var settings = new I2cConnectionSettings(1, 0x3C);
                //var device = I2CDeviceFt260.Create(settings);
                //var s = new Ssd1306(device, 128, 64);

                //using (var display = new Ssd1306(I2CDeviceFt260.Create(new I2cConnectionSettings(1, 0x3C)), 128, 64))
                //{


                //    // Clear the display.
                //    display.ClearScreen();
                //}


                //_ssd1306 = new Ssd1306(I2CDeviceFt260.Create(new I2cConnectionSettings(1, 0x3C)), 128, 64);

                // Display initialisieren
                //_ssd1306.SendCommand(new SetDisplayOff());
                //_ssd1306.SendCommand(new SetDisplayClockDivideRatioOscillatorFrequency(0x00, 0x08));
                //_ssd1306.SendCommand(new SetMultiplexRatio(0x3F)); // 64 Zeilen
                //_ssd1306.SendCommand(new SetDisplayOffset(0x00));
                //_ssd1306.SendCommand(new SetDisplayStartLine(0x00));
                //_ssd1306.SendCommand(new SetChargePump(true));
                //_ssd1306.SendCommand(new SetMemoryAddressingMode(SetMemoryAddressingMode.AddressingMode.Horizontal));
                //_ssd1306.SendCommand(new SetSegmentReMap(true));
                //_ssd1306.SendCommand(new SetComOutputScanDirection(false));
                //_ssd1306.SendCommand(new SetComPinsHardwareConfiguration(false, false));
                //_ssd1306.SendCommand(new SetContrastControlForBank0(0x8F));
                //_ssd1306.SendCommand(new SetPreChargePeriod(0x01, 0x0F));
                //_ssd1306.SendCommand(new SetVcomhDeselectLevel(SetVcomhDeselectLevel.DeselectLevel.Vcc1_00));
                //_ssd1306.SendCommand(new EntireDisplayOn(false));
                //_ssd1306.SendCommand(new SetNormalDisplay());
                //_ssd1306.SendCommand(new SetDisplayOn());


                if (true)
                {
                    _ssd1306 = new Ssd1306(I2CDeviceFt260.Create(new I2cConnectionSettings(1, 0x3C)), 128, 64);

                    _ssd1306.SendCommand(new SetDisplayOff());
                    _ssd1306.SendCommand(new SetDisplayClockDivideRatioOscillatorFrequency());
                    _ssd1306.SendCommand(new SetMultiplexRatio(0x1F));
                    _ssd1306.SendCommand(new SetDisplayOffset());
                    _ssd1306.SendCommand(new SetDisplayStartLine());
                    _ssd1306.SendCommand(new SetChargePump(true));
                    _ssd1306.SendCommand(new SetMemoryAddressingMode(SetMemoryAddressingMode.AddressingMode.Horizontal));
                    _ssd1306.SendCommand(new SetSegmentReMap(true));
                    _ssd1306.SendCommand(new SetComOutputScanDirection(false));
                    _ssd1306.SendCommand(new SetComPinsHardwareConfiguration(false));
                    _ssd1306.SendCommand(new SetContrastControlForBank0(0x8F));
                    _ssd1306.SendCommand(new SetPreChargePeriod(0x01, 0x0F));
                    _ssd1306.SendCommand(new SetVcomhDeselectLevel(SetVcomhDeselectLevel.DeselectLevel.Vcc1_00));
                    _ssd1306.SendCommand(new EntireDisplayOn(false));
                    _ssd1306.SendCommand(new SetNormalDisplay());
                    _ssd1306.SendCommand(new SetDisplayOn());
                    _ssd1306.SendCommand(new SetColumnAddress());
                    _ssd1306.SendCommand(new SetPageAddress(PageAddress.Page1, PageAddress.Page3));
                }

                Ssd1306WriteText(string.Empty);
            }
#pragma warning restore CA2000 // Dispose objects before losing scope
        }

        _sampleRunning = start;
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _ctsClose.Cancel();
                _ctsClose.Dispose();
                _pcf8574?.Dispose();
                _ssd1306?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    /// <summary>
    ///     Text an das OLED
    /// </summary>
    /// <param name="text"></param>
    private void Ssd1306WriteText(string text)
    {
        if (_ssd1306 == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(text))
        {
            _ssd1306.ClearScreen();
            return;
        }

        //if (string.IsNullOrEmpty(text))
        //    text = new string('x', 86);

        _ssd1306.SendCommand(new SetColumnAddress(124));
        _ssd1306.SendCommand(new SetPageAddress(PageAddress.Page0, PageAddress.Page3));

        //21 Zeichen - 128 Pixel Breite gesamt 4 Pixel pro Zeichen + Leepixelreihe
        text = "0";

        List<byte> buf = new List<byte>();
        foreach (char character in text)
        {
            buf.AddRange(BasicFont.GetCharacterBytes(character));
        }

        _ssd1306.SendData(buf.ToArray());
    }


    /// <summary>
    ///     StartBackgroundWorker
    /// </summary>
    private void StartBackgroundWorker()
    {
        if (_backgroundWorking)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            do
            {
                DoSample();

                await Task.Delay(500, _ctsClose.Token).ConfigureAwait(false);
            } while (!_ctsClose.IsCancellationRequested);
        });
    }

    /// <summary>
    ///     Beispiel ausführen
    /// </summary>
    private void DoSample()
    {
        if (!_sampleRunning)
        {
            return;
        }

        if (_pcf8574 != null)
        {
            _pcf8574.WriteByte(_sampleCounter);
        }

        if (_ssd1306 != null)
        {
            string message = DateTime.Now.ToString("G");
            Ssd1306WriteText(message);
        }

        Ft260Base.Gpio_Write(FT260_GPIO.FT260_GPIO_2, _sampleBoolean);
        _sampleCounter++;
        _sampleBoolean = !_sampleBoolean;
    }

    #region Interface Implementations

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}