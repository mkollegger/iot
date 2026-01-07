# Mks.Iot.Ftdi.Ft260

This library provides a .NET interface for the FTDI Chip **FT260** (HID-class USB to UART/I2C Bridge IC). It wraps the native `LibFT260` library and provides easy access to I2C and GPIO functionalities in a C# environment.

## Features

- **I2C Master**: Full support for I2C Master operations including Initialization, Read, Write, and Status checks.
- **GPIO Control**: Support for configuring and controlling GPIO pins (0-5, A-H), including support for specific pin functions.
- **Automatic Native Library Loading**: Automatically extracts and loads the correct architecture version of `LibFT260.dll` (x64, x86, arm64) for Windows.
- **Device Management**: Tools for listing and connecting to FT260 devices.

## Requirements

- **Operating System**: Windows (due to native `LibFT260.dll` dependency).
- **Runtime**: .NET 10 (or compatible depending on build configuration).

## Usage

### Initialization

To start using the library, create an instance using the `Ft260` class. This initializes the connection to the FT260 device.

```csharp
using Mks.Iot.Ftdi.Ft260;

// Initialize the FT260 wrapper
var ft260 = Ft260.Create();
```

### I2C Operations

The library can automatically scan for connected I2C devices or perform read/write operations.

```csharp
// Get list of connected I2C addresses
var devices = Ft260.Ft260Base.GetI2cDevices(true);

foreach (var address in devices)
{
    Console.WriteLine($"Found device at I2C Address: 0x{address:X2}");
}

// Example: Write to I2C device
// Ft260.Ft260Base.I2CMaster_Write(deviceAddress, flag, dataList);
```

### GPIO Control

You can control the General Purpose Input/Output pins available on the FT260.

```csharp
// Check if a pin is configured as GPIO before use (helper method)
Ft260.Ft260Base.GpioCheckConfig(FT260_GPIO.FT260_GPIO_2, true);

// Set direction to Output
Ft260.Ft260Base.Gpio_SetDir(FT260_GPIO.FT260_GPIO_2, Ft260GpioDir.Output);

// Write High (True) or Low (False)
Ft260.Ft260Base.Gpio_Write(FT260_GPIO.FT260_GPIO_2, true);
```

## License

This project is licensed under the MIT License. See the LICENSE file for details.
