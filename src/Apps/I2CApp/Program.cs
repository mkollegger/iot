#region License

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

#endregion

using Iot.Device.Graphics;
using Iot.Device.Graphics.SkiaSharpAdapter;
using Iot.Device.Lm75;
using Iot.Device.Pcx857x;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using Mks.Iot.Ftdi.Ft260;
using Mks.Iot.I2c;
using Mks.Iot.I2c.Devices;
using SkiaSharp;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Device.I2c;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Mks.Common.Ext;

namespace hellopi
{
    /// <summary>
    ///     Beispielprogramm für den Raspberry Pi 5 zur Steuerung von GPIOs und der Onboard-LED.
    /// </summary>
    internal static class Program
    {
        private static ILogger _log = null!;

        /// <summary>
        ///     Haupteinstiegspunkt des Programms.
        ///     Unterstützt einen optionalen --debug Parameter für Remote-Debugging.
        /// </summary>
        /// <param name="args">Kommandozeilenargumente.</param>
        static async Task Main(string[] args)
        {
            var i2cController = Assembly.GetExecutingAssembly()
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(a => a.Key == "I2cController")?.Value;

            ArgumentNullException.ThrowIfNull(i2cController);
            Console.WriteLine($"I2cController: {i2cController}");

            Func<I2cConnectionSettings, I2cDevice> create;
            if (i2cController.Equals("ft260", StringComparison.InvariantCultureIgnoreCase))
            {
                Ft260Wrapper? ft260 = Ft260Device.Create();
                create = I2cDeviceFt260.Create;
            }
            else if (i2cController.Equals("pi", StringComparison.InvariantCultureIgnoreCase))
            {
                create = I2cDevice.Create;
            }
            else
            {
                throw new Exception();
            }

            var dbg = args.Any(a => a.Equals("--debug", StringComparison.OrdinalIgnoreCase));
            Console.WriteLine("Hello, World@Pi with C#!");

            if (!dbg)
            {
                Console.WriteLine("Starte ohne Debugger.");

                var sc = new I2cScan(create);
                var adr = sc.ScanBus();

                byte b = 0;
                int counter = 0;

                if (adr.Contains(0x3C))
                {
                    Console.WriteLine("Found SSD1306 display");

                    using I2cDevice i2CDevice = create(new I2cConnectionSettings(1, 0x3C));
                    using MksSsd1306 device = new MksSsd1306(i2CDevice, EnumMksSsd1306LineModes.LineMode1);

                    device.ClearScreen();
                    DisplayClock(device);

                    BitTest(device);
                }

                if (adr.Contains(0x20))
                {
                    var pcf8574 = new Pcf8574(create(new I2cConnectionSettings(1, 0x20)));
                    pcf8574.WriteByte(0x33);
                }


                do
                {
                    if (adr.Contains(0x48))
                    {
                        var lm75 = new Lm75(create(new I2cConnectionSettings(1, 0x48)));
                        Console.WriteLine($"LM75 Temperature: {lm75.Temperature}");
                    }

                    if (adr.Contains(0x70))
                    {
                        Console.WriteLine($"Set Byte to {b:X2}");

                        //var pca9538 = new Pca8574(create(new I2cConnectionSettings(1, 0x70)));
                        var pca9538 = create(new I2cConnectionSettings(1, 0x70));

                        pca9538.Write([3,0]);
                        pca9538.Write([1, b]);
                        

                        b = (byte)((b == 0) ? 0xff : 0x00);
                    }
                    
                    await Task.Delay(500).ConfigureAwait(true);
                    counter++;
                    if (counter >= 10)
                    {
                        break;
                    }

                } while (true);


                //await BlinkLedAsync().ConfigureAwait(false);
                Console.WriteLine("Programm beendet.");
                return;
            }

            Console.Write("Warte auf Debugger ...");

            // Warten bis Debugger angehängt ist (wichtig für Remote-Debugging via SSH)
            int count = 0;
            while (!Debugger.IsAttached)
            {
                Thread.Sleep(100);
                count++;
                if (count % 10 == 0)
                {
                    Console.Write(".");
                }

                if (count == 600)
                {
                    Console.WriteLine("\nZeitüberschreitung beim Warten auf Debugger.");
                    break;
                }
            }

            Console.WriteLine();

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Debugger angehängt. Fortfahren...");
                //Debugger.Break();
            }


            var scanner = new I2cScan(create);
            scanner.ScanBus();

            // Startet die Blink-Logik
            //await BlinkLedAsync().ConfigureAwait(false);

            Console.WriteLine("Programm beendet.");
        }

        /// <summary>
        ///     Steuert das Blinken einer LED an einem GPIO-Pin und der Onboard-ACT-LED.
        ///     Nutzt den LibGpiodV2Driver für Kompatibilität mit Raspberry Pi OS Bookworm (libgpiod.so.3).
        /// </summary>
        /// <remarks>
        ///     Dokumentation zum Treiber-Problem (libgpiod v1 vs v2):
        ///     https://github.com/dotnet/iot/blob/main/Documentation/gpio-linux-libgpiod.md
        /// </remarks>
        static async Task BlinkLedAsync()
        {
            // GPIO Pin 21 am Header (RP1 Chip auf Pi 5)
            const int ledPin = 21;

            try
            {
                // Auf dem Raspberry Pi 5 wird der RP1 I/O Controller meist als gpiochip4 angesprochen.
                // Wir nutzen explizit den V2 Treiber, da dieser mit libgpiod.so.3 (Standard in Bookworm) kompatibel ist.
#pragma warning disable SDGPIO0001 // Der V2 Treiber ist aktuell noch als 'Experimental' markiert.
                using var driver = new LibGpiodV2Driver(4);
#pragma warning restore SDGPIO0001

                using var controller = new GpioController(driver);

                // Pin für die Ausgabe vorbereiten
                controller.OpenPin(ledPin, PinMode.Output);

                Console.WriteLine($"Blinke LED an Pin {ledPin} und ACT LED...");

                for (int i = 0; i < 10; i++)
                {
                    // GPIO Pin umschalten
                    controller.Write(ledPin, PinValue.High);
                    // Onboard LED via SysFs umschalten
                    _ = await SetActLedAsync(PinValue.High).ConfigureAwait(false);

                    await Task.Delay(250).ConfigureAwait(false);

                    controller.Write(ledPin, PinValue.Low);
                    _ = await SetActLedAsync(PinValue.Low).ConfigureAwait(false);

                    await Task.Delay(250).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Zugriff auf GPIO: {ex.Message}");
                Console.WriteLine("Hinweis: Stellen Sie sicher, dass libgpiod-dev installiert ist und das Programm mit sudo läuft.");
            }
        }


        private static void BitTest(MksSsd1306 ssd1306)
        {
            byte b = 1;
            Stopwatch sw = Stopwatch.StartNew();
            int counter = 0;

            do
            {
                sw.Restart();

                ssd1306.SendCommand(new SetColumnAddress(127));
                ssd1306.SendCommand(new SetPageAddress());
                ssd1306.SendData([b]);

                b = (byte)(b << 1);
                if (b == 0)
                {
                    b = 1;
                }

                sw.Stop();
                _log.TryLogInfo($"Refresh rate: {sw.ElapsedMilliseconds} ms");

                if (counter++ >= 128)
                {
                    break;
                }
            } while (true);
        }

        /// <summary>
        ///     Steuert die Onboard-ACT-LED über das Linux SysFs Interface.
        /// </summary>
        /// <param name="value">PinValue.High für An, PinValue.Low für Aus.</param>
        /// <returns>True, wenn der Zugriff erfolgreich war.</returns>
        /// <remarks>
        ///     Die ACT-LED ist beim Pi 5 oft nicht direkt über libgpiod erreichbar,
        ///     sondern wird über den Kernel-LED-Treiber unter /sys/class/leds/ gesteuert.
        /// </remarks>
        static async Task<bool> SetActLedAsync(PinValue value)
        {
            // Pfad zur Helligkeitssteuerung der ACT-LED (kann je nach OS-Version variieren)
            string ledPath = "/sys/class/leds/ACT/brightness";
            if (!File.Exists(ledPath))
            {
                ledPath = "/sys/class/leds/pwr/brightness";
            }

            if (!File.Exists(ledPath))
            {
                return false;
            }

            try
            {
                // '1' schaltet die LED ein, '0' aus.
                await File.WriteAllTextAsync(ledPath, value == PinValue.High ? "1" : "0").ConfigureAwait(false);
            }
            catch (UnauthorizedAccessException)
            {
                // SysFs Zugriff erfordert in der Regel Root-Rechte (sudo)
                return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


        static private void DisplayClock(GraphicDisplay ssd1306)
        {
            Console.WriteLine("Display clock");
            int fontSize = 12;
            //var font = "DejaVu Sans";
            string font = "Consolas";

            int y = 0;

            using BitmapImage image = BitmapImage.CreateBitmap(128, 64, PixelFormat.Format32bppArgb);
            using SKPaint paint = new SKPaint();

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
            string text = DateTime.Now.ToString("HH:mm:ss") + "\nHello World!";
            _log.TryLogInfo($"({nameof(DisplayClock)}): {text}");
            g.DrawText(text, font, fontSize, Color.White, new Point(0, y));
            ssd1306.DrawBitmap(image);
            _log.TryLogInfo($"({nameof(DisplayClock)}): Done!");
        }
    }
}