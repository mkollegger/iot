using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;

namespace Mks.Iot.I2c.Devices.Pca9538;

/// <summary>
/// MksPca9538 8-bit I/O Expander.
/// Provides GPIO expansion via I2C bus.
/// </summary>
public class MksPca9538 : I2cDevice
{
    private readonly I2cDevice _i2cDevice;
    private MksPca9538GpioDriver? _driver;
    private GpioController? _gpioController;

    private Timer? _pollingTimer;
    private GpioController? _hostGpio; 
    private int _interruptPin;
    private int _lastInputState;
    private readonly object _lock = new object();

    /// <summary>
    /// Initializes a new instance of the <see cref="MksPca9538"/> class.
    /// </summary>
    /// <param name="i2cDevice">The I2C device used for communication.</param>
    public MksPca9538(I2cDevice i2cDevice)
    {
        _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
    }

    /// <inheritdoc/>
    public override I2cConnectionSettings ConnectionSettings => _i2cDevice.ConnectionSettings;

    /// <inheritdoc/>
    public override byte ReadByte() => _i2cDevice.ReadByte();

    /// <inheritdoc/>
    public override void Read(Span<byte> buffer) => _i2cDevice.Read(buffer);

    /// <inheritdoc/>
    public override void WriteByte(byte value) => _i2cDevice.WriteByte(value);

    /// <inheritdoc/>
    public override void Write(ReadOnlySpan<byte> buffer) => _i2cDevice.Write(buffer);

    /// <inheritdoc/>
    public override void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer) => _i2cDevice.WriteRead(writeBuffer, readBuffer);

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            StopMonitoring();
            _gpioController?.Dispose();
            _i2cDevice?.Dispose();
        }
        base.Dispose(disposing);
    }

    // MksPca9538 Specific Functions

    /// <summary>
    /// Reads a byte from the specified register.
    /// </summary>
    /// <param name="register">The register to read from.</param>
    /// <returns>The value read from the register.</returns>
    public byte ReadRegister(MksPca9538Register register)
    {
        Span<byte> writeBuffer = stackalloc byte[] { (byte)register };
        Span<byte> readBuffer = stackalloc byte[1];
        _i2cDevice.WriteRead(writeBuffer, readBuffer);
        return readBuffer[0];
    }

    /// <summary>
    /// Writes a byte to the specified register.
    /// </summary>
    /// <param name="register">The register to write to.</param>
    /// <param name="value">The value to write.</param>
    public void WriteRegister(MksPca9538Register register, byte value)
    {
        Span<byte> writeBuffer = stackalloc byte[] { (byte)register, value };
        _i2cDevice.Write(writeBuffer);
    }

    /// <summary>
    /// Gets or sets the Configuration Register (0x03).
    /// Each bit controls the direction of a pin.
    /// 1 = Input (High-Z), 0 = Output.
    /// </summary>
    public byte Configuration
    {
        get => ReadRegister(MksPca9538Register.Configuration);
        set => WriteRegister(MksPca9538Register.Configuration, value);
    }

    /// <summary>
    /// Gets or sets the Output Port Register (0x01).
    /// Sets the outgoing logic levels of the pins defined as outputs.
    /// </summary>
    public byte OutputPort
    {
        get => ReadRegister(MksPca9538Register.OutputPort);
        set => WriteRegister(MksPca9538Register.OutputPort, value);
    }

    /// <summary>
    /// Gets the Input Port Register (0x00).
    /// Reflects the incoming logic levels of the pins.
    /// </summary>
    public byte InputPort => ReadRegister(MksPca9538Register.InputPort);

    /// <summary>
    /// Gets or sets the Polarity Inversion Register (0x02).
    /// Allows inverting the polarity of the Input Port register data.
    /// </summary>
    public byte PolarityInversion
    {
        get => ReadRegister(MksPca9538Register.PolarityInversion);
        set => WriteRegister(MksPca9538Register.PolarityInversion, value);
    }

    /// <summary>
    /// Gets a GpioController instance for this device.
    /// </summary>
    /// <returns>A GpioController that can be used to control the pins.</returns>
    public GpioController GetGpioController()
    {
        lock (_lock)
        {
            if (_gpioController == null)
            {
                _driver = new MksPca9538GpioDriver(this);
                _gpioController = new GpioController(PinNumberingScheme.Logical, _driver);
            }
            return _gpioController;
        }
    }

    // Polling / Interrupt Logic

    /// <summary>
    /// Enables polling mode to check for input changes.
    /// </summary>
    /// <param name="intervalMs">Pollling interval in milliseconds.</param>
    public void EnablePolling(int intervalMs)
    {
        lock (_lock)
        {
            StopMonitoring();
            // Initialize last state
            _lastInputState = InputPort;
            _pollingTimer = new Timer(OnPoll, null, 0, intervalMs);
        }
    }

    /// <summary>
    /// Enables interrupt mode using a host GPIO pin.
    /// </summary>
    /// <param name="hostGpio">The host GpioController.</param>
    /// <param name="pin">The pin number on the host controller connected to the PCA9538 INT pin.</param>
    public void EnableInterrupt(GpioController hostGpio, int pin)
    {
        lock (_lock)
        {
            StopMonitoring();
            _hostGpio = hostGpio ?? throw new ArgumentNullException(nameof(hostGpio));
            _interruptPin = pin;
            _lastInputState = InputPort; // Initial read

            // Setup interrupt on host
            // PCA9538 INT pin is Open-Drain, Active LOW.
            if (!_hostGpio.IsPinOpen(_interruptPin))
            {
                 _hostGpio.OpenPin(_interruptPin, PinMode.Input); 
            }
            
            _hostGpio.RegisterCallbackForPinValueChangedEvent(_interruptPin, PinEventTypes.Falling, OnHostInterrupt);
        }
    }

    private void StopMonitoring()
    {
        _pollingTimer?.Dispose();
        _pollingTimer = null;

        if (_hostGpio != null)
        {
            _hostGpio.UnregisterCallbackForPinValueChangedEvent(_interruptPin, OnHostInterrupt);
            _hostGpio = null;
        }
    }

    private void OnPoll(object? state)
    {
        CheckChanges();
    }

    private void OnHostInterrupt(object sender, PinValueChangedEventArgs e)
    {
        CheckChanges();
    }

    private void CheckChanges()
    {
         try
         {
             byte currentState = InputPort;
             if (currentState != _lastInputState)
             {
                 int diff = currentState ^ _lastInputState;
                 _lastInputState = currentState;

                 if (_driver != null)
                 {
                     for (int i = 0; i < 8; i++)
                     {
                         if (((diff >> i) & 1) == 1)
                         {
                             // Pin i changed
                             PinEventTypes type = ((currentState >> i) & 1) == 1 ? PinEventTypes.Rising : PinEventTypes.Falling;
                             _driver.NotifyPinChange(i, type);
                         }
                     }
                 }
             }
         }
         catch
         {
             // Ignore errors during checking (e.g. I2C bus error)
         }
    }
}
