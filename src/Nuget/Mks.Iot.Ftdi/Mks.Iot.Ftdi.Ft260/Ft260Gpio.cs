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
using System.Device.Gpio;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mks.Iot.Ftdi.Ft260;

/// <summary>
///     <para>Gpio Driver for Ft260</para>
/// </summary>
public class Ft260Gpio : GpioDriver
{
    private static Ft260Gpio? _ioController;
    private static GpioController? _instance;

    private readonly Dictionary<PinChangeEventHandler, int> _callbacks = new();
    private readonly Dictionary<int, bool> _currentPinValue = new();
    private readonly Ft260Wrapper _ft260;
    private Task? _eventWorker;

    internal Ft260Gpio(Ft260Wrapper ft260)
    {
        ArgumentNullException.ThrowIfNull(ft260);
        _ft260 = ft260;
    }

    #region Properties

    /// <inheritdoc />
    protected override int PinCount => 14;

    #endregion

    /// <summary>
    ///     Creates a GpioController instance using Ft260
    /// </summary>
    /// <returns>GpioController</returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public static GpioController Create()
    {
        if (_instance == null)
        {
            _ioController = new Ft260Gpio(Ft260.Create());
            _instance = new GpioController(_ioController);
        }

        return _instance;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _ft260.Dispose();
            _ioController = null;
            _instance = null;
        }
    }


    /// <inheritdoc />
    protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
    {
        //The FT260 contains 14 GPIO pins. Each GPIO pin is multiplexed with other functions as listed below:
        //  0 => GPIO0 / SCL
        //  1 => GPIO1 / SDA
        //  2 => GPIO2 / SUSPEND OUT / TX_LED / PWREN
        //  3 => GPIO3 / WAKEUP / INTR
        //  4 => GPIO4 / UART DCD
        //  5 => GPIO5 / UART RI
        //  6 => GPIOA / TX_ACTIVE / TX_LED / PWREN
        //  7 => GPIOB / UART_RTS_N
        //  8 => GPIOC / UART_RXD
        //  9 => GPIOD / UART_TXD
        //  10 => GPIOE / UART_CTS_N
        //  11 => GPIOF / UART_DTR_N
        //  12 => GPIOG / BCD_DET / RX_LED
        //  13 => GPIOH / UART_DST_N
        FT260_GPIO pin = ConvertToFt260Gpio(pinNumber);
        return (int) pin;
    }

    /// <inheritdoc />
    protected override void OpenPin(int pinNumber)
    {
        FT260_GPIO p = ConvertToFt260Gpio(pinNumber);
        if (p == FT260_GPIO.FT260_GPIO_2)
        {
            _ft260.SelectGpio2Function(FT260_GPIO2_Pin.FT260_GPIO2_GPIO);
        }
        else if (p == FT260_GPIO.FT260_GPIO_A)
        {
            _ft260.SelectGpioAFunction(FT260_GPIOA_Pin.FT260_GPIOA_GPIO);
        }
        else if (p == FT260_GPIO.FT260_GPIO_G)
        {
            _ft260.SelectGpioGFunction(FT260_GPIOG_Pin.FT260_GPIOG_GPIO);
        }

        _ft260.GpioCheckConfig(p);
    }

    /// <inheritdoc />
    protected override void ClosePin(int pinNumber)
    {
        //FT260_GPIO p = ConvertToFt260Gpio(pinNumber);
    }

    /// <inheritdoc />
    protected override void SetPinMode(int pinNumber, PinMode mode)
    {
        FT260_GPIO p = ConvertToFt260Gpio(pinNumber);
        switch (mode)
        {
            case PinMode.Input:
                _ft260.Gpio_SetDir(p, Ft260GpioDir.Input);
                break;
            case PinMode.Output:
                _ft260.Gpio_SetDir(p, Ft260GpioDir.Output);
                break;
            case PinMode.InputPullDown:
            case PinMode.InputPullUp:
                throw new NotSupportedException($"Pin mode {mode} is not supported by Ft260.");
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    /// <inheritdoc />
    protected override PinMode GetPinMode(int pinNumber)
    {
        FT260_GPIO p = ConvertToFt260Gpio(pinNumber);
        FT260_GPIO_Report? tmp = _ft260.Gpio_Get();
        if (tmp == null || !tmp.HasValue)
        {
            throw new InvalidOperationException("Failed to get GPIO configuration.");
        }

        int set;
        if (pinNumber <= 5)
        {
            set = tmp.Value.dir & (ushort) p;
        }
        else
        {
            set = tmp.Value.gpioN_dir & (ushort) p;
        }

        return set == 0 ? PinMode.Input : PinMode.Output;
    }

    /// <inheritdoc />
    protected override bool IsPinModeSupported(int pinNumber, PinMode mode)
    {
        if (mode == PinMode.Input || mode == PinMode.Output)
        {
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    protected override PinValue Read(int pinNumber)
    {
        FT260_GPIO p = ConvertToFt260Gpio(pinNumber);
        bool? val = _ft260.Gpio_Read(p);
        if (val == null || !val.HasValue)
        {
            throw new InvalidOperationException("Failed to read GPIO value.");
        }

        return val.Value ? PinValue.High : PinValue.Low;
    }

    /// <inheritdoc />
    protected override void Write(int pinNumber, PinValue value)
    {
        FT260_GPIO p = ConvertToFt260Gpio(pinNumber);
        _ft260.Gpio_Write(p, value == PinValue.High);
    }

    /// <inheritdoc />
    protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
    {
        _callbacks.Add(callback, pinNumber);

        bool? currentValue = _ft260.Gpio_Read(ConvertToFt260Gpio(pinNumber));
        if (currentValue == null || !currentValue.HasValue)
        {
            throw new InvalidOperationException("Failed to read GPIO value for event callback.");
        }

        if (!_currentPinValue.TryAdd(pinNumber, currentValue.Value))
        {
            _currentPinValue[pinNumber] = currentValue.Value;
        }

        if (_eventWorker == null)
        {
            _eventWorker = new Task(async () =>
            {
                while (_callbacks.Count > 0)
                {
                    List<int> pins = _callbacks.Values.Distinct().ToList();
                    foreach (int pin in pins)
                    {
                        bool? val = _ft260.Gpio_Read(ConvertToFt260Gpio(pin));
                        if (val == null || !val.HasValue)
                        {
                            throw new InvalidOperationException("Failed to read GPIO value for event callback.");
                        }

                        if (_currentPinValue.TryGetValue(pin, out bool cv) && cv != val.Value)
                        {
                            _currentPinValue[pin] = val.Value;
                            PinEventTypes changeType = val.Value ? PinEventTypes.Rising : PinEventTypes.Falling;
                            foreach (PinChangeEventHandler cb in _callbacks
                                .Where(w => w.Value == pin)
                                .Select(s => s.Key))
                            {
                                cb?.Invoke(this, new PinValueChangedEventArgs(changeType, pin));
                            }
                        }
                    }

                    await Task.Delay(50).ConfigureAwait(false); // Adjust the delay as necessary
                }
            });
            _eventWorker.Start();
        }


        //callback?.Invoke(this,new PinValueChangedEventArgs(eventTypes,pinNumber));
        //throw new NotImplementedException();
    }

    /// <inheritdoc />
    protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
    {
        _callbacks.Remove(callback);
        if (_callbacks.Count == 0 && _eventWorker != null)
        {
            _eventWorker.Dispose();
            _eventWorker = null;
        }

        //throw new NotImplementedException();
    }


    private FT260_GPIO ConvertToFt260Gpio(int pinNumber)
    {
        if (pinNumber < 0 || pinNumber > 13)
        {
            throw new ArgumentOutOfRangeException(nameof(pinNumber), "Pin number must be between 0 and 13 for Ft260.");
        }

        switch (pinNumber)
        {
            case 0:
                return FT260_GPIO.FT260_GPIO_0; // GPIO0 / SCL
            case 1:
                return FT260_GPIO.FT260_GPIO_1; // GPIO1 / SDA
            case 2:
                return FT260_GPIO.FT260_GPIO_2; // GPIO2 / SUSPEND OUT / TX_LED / PWREN
            case 3:
                return FT260_GPIO.FT260_GPIO_3; // GPIO3 / WAKEUP / INTR
            case 4:
                return FT260_GPIO.FT260_GPIO_4; // GPIO4 / UART DCD
            case 5:
                return FT260_GPIO.FT260_GPIO_5; // GPIO5 / UART RI
            case 6:
                return FT260_GPIO.FT260_GPIO_A; // GPIOA / TX_ACTIVE / TX_LED / PWREN
            case 7:
                return FT260_GPIO.FT260_GPIO_B; // UART_RTS_N
            case 8:
                return FT260_GPIO.FT260_GPIO_C; // UART_RXD
            case 9:
                return FT260_GPIO.FT260_GPIO_D; // UART_TXD
            case 10:
                return FT260_GPIO.FT260_GPIO_E; // UART_CTS_N
            case 11:
                return FT260_GPIO.FT260_GPIO_F; // UART_DTR_N
            case 12:
                return FT260_GPIO.FT260_GPIO_G; // GPIOG / BCD_DET / RX_LED
            case 13:
                return FT260_GPIO.FT260_GPIO_H; // GPIOH / UART_DST_N
            default:
                throw new ArgumentOutOfRangeException(nameof(pinNumber), "Pin number must be between 0 and 13 for Ft260.");
        }
    }
}