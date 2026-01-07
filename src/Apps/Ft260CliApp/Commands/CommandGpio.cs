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
using System.Device.Gpio;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mks.Common.Ext;
using Mks.Iot.Ftdi.Ft260;

namespace Ft260CliApp.Commands;

public class CommandGpio : Command
{
    public CommandGpio() : base("--gpio", "Execute GPIO operations")
    {
        this.SetHandler(async context =>
        {
            var host = context.GetHost();
            var log = host.Services.GetRequiredService<ILogger<CommandGpio>>();

            log.TryLogInformation("GPIO command executed");

            Console.WriteLine("Executing GPIO command...");
            Console.WriteLine("LED on GPIO pin 2 flashes (output)");
            Console.WriteLine("GPIO on pin 3 shows changes (input)");
            Console.WriteLine("Press any key to stop - automatic stops in 10s");

            using var gpio = Ft260Gpio.Create();
            var pinLed = gpio.OpenPin(2, PinMode.Output);
            var pinInput = gpio.OpenPin(3, PinMode.Input);

            pinInput.ValueChanged += OnPinInputOnValueChanged;

            int counter = 0;
            do
            {
                await Task.Delay(250);
                pinLed.Toggle();
                counter++;
                if (Console.KeyAvailable || counter >= 40)
                {
                    break;
                }
            } while (true);

            //Detach event handler ValueChanged
            pinInput.ValueChanged -= OnPinInputOnValueChanged;
        });
    }

    private void OnPinInputOnValueChanged(object sender, PinValueChangedEventArgs eventArgs)
    {
        Console.WriteLine($"Pin {eventArgs.PinNumber} changed to {eventArgs.ChangeType}");
    }
}