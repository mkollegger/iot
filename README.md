# iot

![MKS Logo](doc/mks. png)

C# libraries for embedded systems, including chip support and hardware abstractions.  Provides installation guides for platforms like Raspberry Pi, Arduino Meadow, and other embedded devices, with a strong focus on modern C# development.

## 📋 Overview

This repository provides a comprehensive collection of . NET libraries and tools for IoT development, focusing on: 

- **Hardware Abstractions**: Cross-platform GPIO, I2C, SPI, and other hardware protocols
- **Chip Support**: Device bindings for various sensors, displays, and peripherals
- **Platform Support**:  Raspberry Pi, Arduino Meadow, ESP32, and other embedded systems
- **Modern . NET**:  Built with .NET 9.0 and following modern C# best practices

## 🏗️ Repository Structure

```
.
├── src/                    # Source code and libraries
│   ├── Nuget/             # NuGet packages
│   │   ├── Mks.Iot.I2c/           # I2C hardware abstraction library
│   │   └── Mks.Iot.Ftdi/          # FTDI chip support (FT260)
│   └── Tests/             # Unit and integration tests
├── samples/               # Sample projects and examples
│   └── hellopi/          # Hello World example for Raspberry Pi
├── doc/                   # Documentation and resources
└── Changelog.md          # Version history and changes
```

## 📦 Available Libraries

### Mks.Iot.I2c
I2C hardware abstraction library providing easy-to-use interfaces for I2C communication with various devices.

**Features:**
- Cross-platform I2C support
- Built on System.Device. Gpio and Iot.Device.Bindings
- Comprehensive error handling

### Mks.Iot. Ftdi. Ft260
Support for FTDI FT260 USB-to-I2C/UART bridge chip.

**Features:**
- Native library integration for Windows (x86, x64, ARM64)
- Direct hardware access
- Low-level device control

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- Visual Studio 2026 or Visual Studio Code
- Target hardware (e.g., Raspberry Pi 4 or newer)

### Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/mkollegger/iot.git
   cd iot
   ```

2. **Explore the samples**
   ```bash
   cd samples/hellopi
   dotnet build
   dotnet run
   ```

3. **Check the documentation**
   Visit the [project Wiki](https://github.com/mkollegger/iot/wiki) for detailed guides and tutorials.

## 📚 Documentation

For detailed information, guides, and additional documentation, please visit the **[project Wiki](https://github.com/mkollegger/iot/wiki)**.

The Wiki contains:
- **Installation guides** for various platforms (Raspberry Pi, Arduino Meadow, ESP32)
- **Configuration instructions** for development environments
- **Usage examples** and code samples
- **Troubleshooting tips** for common issues
- **API documentation** for the libraries
- **Best practices** for IoT development with .NET

Additional documentation: 
- [Sample Projects](samples/readme.md) - Overview of available sample projects
- [Documentation Resources](doc/readme.md) - Additional documentation files

## 🎯 Sample Projects

### [HelloPi](samples/hellopi/)

A simple starter example demonstrating the basics of Raspberry Pi development with .NET.

**Features:**
- Control of 2 LEDs via GPIO
- Remote deployment and debugging
- Visual Studio Code and Visual Studio 2026 support
- Simple project structure for getting started

[→ View all sample projects](samples/readme.md)

## 🌐 Supported Platforms

### IoT Devices
- Raspberry Pi (4 and newer)
- Arduino Meadow
- ESP32 and other embedded systems

### Desktop
- Windows (x86, x64, ARM64)
- macOS
- Linux

### Mobile
- Android tablets and smartphones
- iOS devices

### Cloud & Edge
- Containers and cloud infrastructure
- Edge computing scenarios

## 🛠️ Development

### Building the Solution

```bash
cd src
dotnet build Iot.slnx
```

### Running Tests

```bash
cd src/Tests
dotnet test
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues, feature requests, or pull requests.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🔗 Links

- **Wiki**: [https://github.com/mkollegger/iot/wiki](https://github.com/mkollegger/iot/wiki)
- **Issues**: [https://github.com/mkollegger/iot/issues](https://github.com/mkollegger/iot/issues)
- **Samples**: [Sample Projects Documentation](samples/readme.md)

## 📞 Contact

For questions and support, please: 
- Check the [Wiki](https://github.com/mkollegger/iot/wiki)
- Open an [issue](https://github.com/mkollegger/iot/issues)
- Visit the [project discussions](https://github.com/mkollegger/iot/discussions)

---

**Made with ❤️ for the IoT and .NET community**
