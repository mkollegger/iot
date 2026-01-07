# Mks.Iot.I2c

This library provides I2C device drivers and abstractions for .NET IoT applications. Currently, it features an enhanced driver for the **SSD1306** OLED display.

## Features

### MksSsd1306
An enhanced `Ssd1306` driver implementation that simplifies text rendering on 128x32 OLED displays.

- **Text Rendering**: Uses **SkiaSharp** for high-quality text rendering.
- **Line Modes**: Supports different text layouts:
  - **4 Lines**: ~25 characters per line (Small font).
  - **2 Lines**: ~13 characters per line (Medium font).
  - **1 Line**: ~8 characters per line (Large font).
- **Alignment**: specific text alignment support (Left, Center, Right).
- **Drawing**: Built on top of `Iot.Device.Ssd13xx`, allowing standard drawing operations.

## Requirements

- **Runtime**: .NET 10 (or compatible).
- **Dependencies**: `Iot.Device.Bindings`, `SkiaSharp`.

## Usage

### Initialization

To use `MksSsd1306`, you need an initialized `I2cDevice`.

```csharp
using System.Device.I2c;
using Mks.Iot.I2c.Devices;

// Create I2C device (e.g., creating a connection to bus 1, device address 0x3C)
var i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, 0x3C));

// Initialize MksSsd1306 with 2-line mode
using var display = new MksSsd1306(i2cDevice, EnumMksSsd1306LineModes.LineMode2);
```

### Writing Text

You can write text to specific rows with alignment options.

```csharp
using SkiaSharp; // For SKTextAlign

// Write "Hello" on the first row, aligned left
display.WriteText("Hello", 0, SKTextAlign.Left);

// Write "World" on the second row, aligned center
display.WriteText("World", 1, SKTextAlign.Center);
```

### Changing Line Modes

You can dynamically change the line layout (font size automatically adjusts).

```csharp
// Switch to 1-line mode (Large font)
display.LineMode = EnumMksSsd1306LineModes.LineMode1;
display.WriteText("Big Text", 0, SKTextAlign.Center);
```

### Clearing Screen

```csharp
// Clear a specific line
display.ClearLine(0);

// Clear the entire screen
display.ClearScreen();
```

## License

This project is licensed under the MIT License. See the LICENSE file for details.
