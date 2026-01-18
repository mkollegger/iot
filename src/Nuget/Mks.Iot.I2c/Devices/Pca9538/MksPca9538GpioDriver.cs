using System;
using System.Collections.Concurrent;
using System.Device.Gpio;
using System.Threading;

namespace Mks.Iot.I2c.Devices.Pca9538;

/// <summary>
/// GPIO Driver for MksPca9538.
/// Handles GPIO operations for the 8-bit I/O Expander.
/// </summary>
internal class MksPca9538GpioDriver : GpioDriver
{
    private readonly MksPca9538 _owner;
    private readonly ConcurrentDictionary<int, PinChangeEventHandler?> _callbacks = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MksPca9538GpioDriver"/> class.
    /// </summary>
    /// <param name="owner">The MksPca9538 instance owning this driver.</param>
    public MksPca9538GpioDriver(MksPca9538 owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    /// <inheritdoc/>
    protected override int PinCount => 8;

    /// <inheritdoc/>
    protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber) => pinNumber;

    /// <inheritdoc/>
    protected override void OpenPin(int pinNumber)
    {
        if (pinNumber < 0 || pinNumber > 7)
            throw new ArgumentOutOfRangeException(nameof(pinNumber), "Pin number must be between 0 and 7.");
    }

    /// <inheritdoc/>
    protected override void ClosePin(int pinNumber)
    {
        // No specific action needed for closing a pin on this expader
    }

    /// <inheritdoc/>
    protected override void SetPinMode(int pinNumber, PinMode mode)
    {
        byte config = (byte)_owner.Configuration;
        if (mode == PinMode.Input)
        {
            config |= (byte)(1 << pinNumber);
        }
        else if (mode == PinMode.Output)
        {
            config &= (byte)~(1 << pinNumber);
        }
        else
        {
            throw new NotSupportedException($"Pin mode {mode} is not supported.");
        }
        _owner.Configuration = config;
    }

    /// <inheritdoc/>
    protected override PinValue Read(int pinNumber)
    {
        byte val = (byte)_owner.InputPort;
        return ((val >> pinNumber) & 1) == 1 ? PinValue.High : PinValue.Low;
    }

    /// <inheritdoc/>
    protected override void Write(int pinNumber, PinValue value)
    {
        byte output = (byte)_owner.OutputPort;
        if (value == PinValue.High)
            output |= (byte)(1 << pinNumber);
        else
            output &= (byte)~(1 << pinNumber);
        _owner.OutputPort = output;
    }


    /// <inheritdoc/>
    protected override bool IsPinModeSupported(int pinNumber, PinMode mode)
    {
        return mode == PinMode.Input || mode == PinMode.Output;
    }

    /// <inheritdoc/>
    protected override PinMode GetPinMode(int pinNumber)
    {
        byte config = (byte)_owner.Configuration;
        return ((config >> pinNumber) & 1) == 1 ? PinMode.Input : PinMode.Output;
    }

    /// <inheritdoc/>
    protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
    {
        _callbacks.AddOrUpdate(pinNumber, callback, (k, v) => v + callback);
    }

    /// <inheritdoc/>
    protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
    {
        _callbacks.AddOrUpdate(pinNumber, null, (k, v) => v - callback);
    }

    /// <inheritdoc/>
    protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
    {
        using ManualResetEventSlim eventWaitHandle = new ManualResetEventSlim(false);
        PinEventTypes detectedEventType = PinEventTypes.None;

        void Callback(object sender, PinValueChangedEventArgs e)
        {
            if ((e.ChangeType & eventTypes) != 0)
            {
                detectedEventType = e.ChangeType;
                try
                {
                    eventWaitHandle.Set();
                }
                catch (ObjectDisposedException)
                {
                    // Ignore if disposed during race condition
                }
            }
        }

        AddCallbackForPinValueChangedEvent(pinNumber, eventTypes, Callback);

        try
        {
            eventWaitHandle.Wait(cancellationToken);
            return new WaitForEventResult { EventTypes = detectedEventType, TimedOut = false };
        }
        catch (OperationCanceledException)
        {
            return new WaitForEventResult { EventTypes = PinEventTypes.None, TimedOut = true };
        }
        finally
        {
            RemoveCallbackForPinValueChangedEvent(pinNumber, Callback);
        }
    }

    /// <summary>
    /// Notifies the driver that a pin value has changed (called by the owner).
    /// </summary>
    /// <param name="pin">The pin number.</param>
    /// <param name="type">The type of event (Rising/Falling).</param>
    public void NotifyPinChange(int pin, PinEventTypes type)
    {
        if (_callbacks.TryGetValue(pin, out var callback) && callback != null)
        {
            callback.Invoke(this, new PinValueChangedEventArgs(type, pin));
        }
    }
}
