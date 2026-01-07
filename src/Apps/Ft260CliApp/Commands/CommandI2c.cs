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

using System.CommandLine;
using System.CommandLine.Hosting;
using System.Device.I2c;
using System.Diagnostics;
using System.Drawing;
using Iot.Device.Graphics;
using Iot.Device.Graphics.SkiaSharpAdapter;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mks.Common.Ext;
using Mks.Iot.Ftdi.Ft260;
using Mks.Iot.I2c.Devices;
using SkiaSharp;

namespace Ft260CliApp.Commands;

public class CommandI2C : Command
{
    private ILogger<CommandI2C>? _log;

    public CommandI2C() : base("--i2c", "Execute I2C operations")
    {
        // Setup arguments and options here

        this.SetHandler(context =>
        {
            var host = context.GetHost();
            _log = host.Services.GetRequiredService<ILogger<CommandI2C>>();

            _log.TryLogTrace($"[{GetType().Name}]({nameof(CommandI2C)}): I2C command executed");

            var ft260 = Ft260.Create();
            Console.Write("I2C devices found: ");
            foreach (var d in ft260.GetI2cDevices())
            {
                Console.Write($"0x{d:X2} ");
            }

            Console.WriteLine();

            if (ft260.GetI2cDevices().Contains(0x3C))
            {
                Console.WriteLine("Found SSD1306 display");

                using I2cDevice i2CDevice = I2CDeviceFt260.Create(new I2cConnectionSettings(1, 0x3C));
                using MksSsd1306 device = new MksSsd1306(i2CDevice, EnumBissSsd1306LineModes.LineMode1);

                device.ClearScreen();
                DisplayClock(device);

                BitTest(device);
            }
        });
    }

    private void DisplayClock(GraphicDisplay ssd1306)
    {
        Console.WriteLine("Display clock");
        int fontSize = 12;
        //var font = "DejaVu Sans";
        string font = "Consolas";

        int y = 0;

        using BitmapImage image = BitmapImage.CreateBitmap(128, 64, PixelFormat.Format32bppArgb);
        using var paint = new SKPaint();

        paint.Color = SKColors.White;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 1;
        paint.TextEncoding = SKTextEncoding.Utf32;
        paint.TextSize = 14;
        paint.Typeface = SKTypeface.FromFamilyName(font, SKFontStyle.Normal);
        paint.TextAlign = SKTextAlign.Left;
        paint.IsAntialias = true;
        paint.HintingLevel = SKPaintHinting.Normal;

        image.Clear(Color.Black);
        IGraphics g = image.GetDrawingApi();
        string text = DateTime.Now.ToString("HH:mm:ss") + "\nHallo Welt!";
        _log.TryLogInfo($"({nameof(DisplayClock)}): {text}");
        g.DrawText(text, font, fontSize, Color.White, new Point(0, y));
        ssd1306.DrawBitmap(image);
        _log.TryLogInfo($"({nameof(DisplayClock)}): Done!");
    }

    private void DisplayImages(GraphicDisplay ssd1306)
    {
        Console.WriteLine("Display Images");
        foreach (string imageName in Directory.GetFiles("images", "*.bmp").OrderBy(f => f))
        {
            using BitmapImage image = BitmapImage.CreateFromFile(imageName);
            ssd1306.DrawBitmap(image);
            Thread.Sleep(1000);
        }
    }

    private void BitTest(MksSsd1306 ssd1306)
    {
        byte[] data = new byte[128];

        byte b = 1;
        Stopwatch sw = Stopwatch.StartNew();

        int counter = 0;

        do
        {
            sw.Restart();

            ssd1306.SendCommand(new SetColumnAddress(127));
            ssd1306.SendCommand(new SetPageAddress());
            ssd1306.SendData([b]);

            b = (byte) (b << 1);
            if (b == 0)
            {
                b = 1;
            }

            sw.Stop();
            _log.TryLogInfo($"Refrash rate: {sw.ElapsedMilliseconds} ms");

            if (counter++ >= 128)
            {
                break;
            }
        } while (true);
    }
}