# FT260 CLI Application

This is a Command Line Interface (CLI) application for interacting with the FTDI FT260 chip, utilizing the `Mks.Iot.Ftdi.Ft260` library. It provides commands for I2C, Serial, and GPIO operations.

## Features

- **I2C Operations**: Scans for devices and runs demos (e.g., SSD1306 OLED, PCF8574).
- **GPIO Operations**: Tests GPIO pins with input and output scenarios.
- **Serial Operations**: (Coming soon) Interface for UART communication.
- **Cross-Platform**: Built on .NET 10.

## Usage

Run the application via the command line.

```bash
Ft260CliApp.exe [command] [options]
```

### Commands

| Command    | Description                                                                 |
|------------|-----------------------------------------------------------------------------|
| `--i2c`    | Execute I2C operations. Scans the bus for devices and runs a display demo if an SSD1306 (0x3C) is found. |
| `--gpio`   | Execute GPIO operations. Blinks an LED on GPIO 2 and monitors input changes on GPIO 3 for 10 seconds. |
| `--serial` | Execute Serial operations. (Currently not implemented).                     |

### Options

| Option             | Description                               |
|--------------------|-------------------------------------------|
| `-?`, `-h`, `--help` | Show help and usage information.          |
| `--version`        | Show version information.                 |

## Examples

**Run I2C Tests:**
```bash
Ft260CliApp.exe --i2c
```

**Run GPIO Tests:**
```bash
Ft260CliApp.exe --gpio
```

## Requirements

- **Hardware**: An FT260 device connected via USB.
- **Drivers**: FTDI drivers installed (LibFT260 is handled by the application).
- **Wiring for Tests**:
  - **I2C**: SSD1306 Display at address `0x3C` (Optional).
  - **GPIO**: LED on Pin 2, Switch/Input on Pin 3 (Optional).

## License

This project is licensed under the MIT License. See the LICENSE file for details.
