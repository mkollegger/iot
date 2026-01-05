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

using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using Iot.Device.Graphics;
using Iot.Device.Graphics.SkiaSharpAdapter;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using Microsoft.Extensions.Logging;
using Mks.Common.Ext;
using Mks.Iot.Ftdi.Ft260;
using Mks.Iot.I2c.Devices;
using SkiaSharp;
using Color = System.Drawing.Color;

namespace I2cTests
{
    internal abstract class Program
    {
        #region Nested Types

        public class Test : SerialPort
        {
            public void StartStopSample()
            {
                Write("");
            }

            public new void Write(string text)
            {
            }
        }

        #endregion

        private static ILogger? _log;

        //https://github.com/dotnet/iot/blob/main/src/devices/Ssd13xx/samples/i2c/Ssd13xx.Samples
        private static async Task Main(string[] args)
        {
            using GpioController gpio = Ft260Gpio.Create();

            GpioPin pin = gpio.OpenPin(2, PinMode.Output);
            pin.Write(PinValue.Low);

            pin.Write(PinValue.High);

            pin.Toggle();

            GpioPin pin2 = gpio.OpenPin(3, PinMode.Input);
            pin2.ValueChanged += (sender, eventArgs) => { Console.WriteLine($"Pin {eventArgs.PinNumber} changed to {eventArgs.ChangeType}"); };

            pin2.ValueChanged += (sender, eventArgs) => { Console.WriteLine($"XXX Pin {eventArgs.PinNumber} changed to {eventArgs.ChangeType}"); };


            //do
            //{
            //    var value = pin2.Read();
            //    Console.WriteLine($"Pin 3 value: {value}");
            //    await Task.Delay(500).ConfigureAwait(true);

            //} while (true);

            //using var driver = new Ft260GpioDriver();
            //using var gpio = new GpioController(driver);


            //using var ft = Ft260.Create();
            //ft.GpioCheckConfig(FT260_GPIO.FT260_GPIO_2);
            //ft.Gpio_SetDir(FT260_GPIO.FT260_GPIO_2, Ft260GpioDir.Output);
            //var r = ft.Gpio_Get();


            //ft.Gpio_Write(FT260_GPIO.FT260_GPIO_2, true);
            //var t = ft.Gpio_Read(FT260_GPIO.FT260_GPIO_2);

            //ft.Gpio_Write(FT260_GPIO.FT260_GPIO_2, false);
            //var f = ft.Gpio_Read(FT260_GPIO.FT260_GPIO_2);


            //SkiaSharpAdapter.Register();

            using I2cDevice i2cDevice = I2CDeviceFt260.Create(new I2cConnectionSettings(1, 0x3C));
            using BissSsd1306 device = new BissSsd1306(i2cDevice, EnumBissSsd1306LineModes.LineMode1);


            device.ClearScreen();


            byte[] data = new byte[128];

            byte b = 1;
            Stopwatch sw = Stopwatch.StartNew();

            do
            {
                sw.Restart();

                //for (int i = 0; i < 4; i++)
                //{
                //    data[0] = i == 0 ? b : (byte)0;
                //    device.SendCommand(new SetColumnAddress());
                //    device.SendCommand(new SetPageAddress((PageAddress)i));
                //    device.SendData(data);
                //}

                device.SendCommand(new SetColumnAddress(127));
                device.SendCommand(new SetPageAddress());
                device.SendData([b]);

                b = (byte) (b << 1);
                if (b == 0)
                {
                    b = 1;
                }

                sw.Stop();
                _log.TryLogInfo($"Refrash rate: {sw.ElapsedMilliseconds} ms");
            } while (true);


            Debugger.Break();


            int counter0 = 0;
            int counter1 = 100000;


            Task.Run(async () =>
            {
                do
                {
                    string dateTime = DateTime.Now.ToLongTimeString();
                    device.WriteText(dateTime, 0, SKTextAlign.Center);
                    await Task.Delay(700).ConfigureAwait(true);
                } while (true);
            });

            do
            {
                await Task.Delay(500).ConfigureAwait(true);
                //device.WriteText(counter0.ToString(),1,SKTextAlign.Center);
                //device.WriteText(counter1.ToString(), 1);
                //device.WriteText(counter0.ToString(), 2);
                //device.WriteText(counter1.ToString(), 3);
                counter0++;
                counter1--;
            } while (true);


            //var h = device.ScreenHeight;
            //var w = device.ScreenWidth;


            //device.ClearScreen();
            ////DisplayImages(device);
            //DisplayClock(device);
            //device.ClearScreen();

            ////SendMessage(device, "Hello .NET IoT!!!");

            using SKFont fnt = new SKFont(SKTypeface.FromFamilyName("Courier New"), 16);
            //fnt.Hinting = SKFontHinting.Full;

            using SKPaint? paint = new SKPaint(fnt)
            {
                Color = SKColors.White,
                TextEncoding = SKTextEncoding.Utf16,
                IsAntialias = false,
                IsLinearText = true,
                IsStroke = false
            };

            int counter = 0;

            do
            {
                string text = counter.ToString();
                counter++;

                text = "012345678912345";

                int textWidth = Convert.ToInt32(Math.Round(paint.MeasureText(text), 0));

                using BitmapImage? image = BitmapImage.CreateBitmap(128, 16, PixelFormat.Format32bppArgb);
                image.Clear(Color.Black);

                IGraphics? i = image.GetDrawingApi();
                SKCanvas? canvas = i.GetCanvas();

                canvas.DrawText(text, 0, 16, paint);

                //canvas.DrawLine(0, 0, 127, 31, paint);
                //i.DrawText(text, "Lucida Console", 8, Color.White, new Point(0, 8));

                device.SendCommand(new SetColumnAddress());
                device.SendCommand(new SetPageAddress(PageAddress.Page2));

                //var bytes = image.AsByteSpan();

                byte[]? bytes = device.PreRenderBitmap(image);
                device.SendData(bytes);

                //await Task.Delay(100);
                //device.DrawBitmap(image);
            } while (true);

            Debugger.Break();

            Console.WriteLine("ENTER to end!");
            Console.ReadLine();
            return;


            using Ft260? test = new Ft260();
            test.StartStopSample();

            //var u = new UsbI2CInterface.UsbI2C();
            //u.GetStatus();


            Console.WriteLine("ENTER to end!");
            Console.ReadLine();
        }


        private static void DisplayImages(GraphicDisplay ssd1306)
        {
            Console.WriteLine("Display Images");
            foreach (string image_name in Directory.GetFiles("images", "*.bmp").OrderBy(f => f))
            {
                using BitmapImage image = BitmapImage.CreateFromFile(image_name);
                ssd1306.DrawBitmap(image);
                Thread.Sleep(1000);
            }
        }

        private static void DisplayClock(GraphicDisplay ssd1306)
        {
            Console.WriteLine("Display clock");
            int fontSize = 12;
            //var font = "DejaVu Sans";
            string font = "Consolas";

            int y = 0;

            while (!Console.KeyAvailable)
            {
                using (BitmapImage image = BitmapImage.CreateBitmap(128, 64, PixelFormat.Format32bppArgb))
                {
                    using SKPaint paint = new SKPaint
                    {
                        Color = SKColors.White,
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 1,
                        TextEncoding = SKTextEncoding.Utf32,
                        TextSize = 14,
                        Typeface = SKTypeface.FromFamilyName(font, SKFontStyle.Normal),
                        TextAlign = SKTextAlign.Left,
                        IsAntialias = true,
                        HintingLevel = SKPaintHinting.Normal
                    };

                    image.Clear(Color.Black);
                    IGraphics g = image.GetDrawingApi();
                    SKCanvas x = g.GetCanvas();
                    //x.DrawLine(0,0,128,0, paint);
                    //x.DrawLine(0, 64, 0, 0, paint);
                    //x.DrawLine(0, 0, 0, 0, paint);
                    //x.DrawLine(0, 0, 0, 0, paint);

                    //x.DrawText(DateTime.Now.ToString("HH:mm:ss"),0,0,paint);
                    string text = DateTime.Now.ToString("HH:mm:ss") + "\nHallo Welt!";
                    _log.TryLogInfo($"({nameof(DisplayClock)}): {text}");
                    g.DrawText(text, font, fontSize, Color.White, new Point(0, y));
                    ssd1306.DrawBitmap(image);
                    _log.TryLogInfo($"({nameof(DisplayClock)}): Done!");

                    //y++;
                    //if (y >= image.Height)
                    //{
                    //    y = 0;
                    //}

                    Thread.Sleep(100);
                }
            }

            Console.ReadKey(true);
        }
    }
}