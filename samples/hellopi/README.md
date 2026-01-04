# HelloPi - Raspberry Pi LED Example

> **This document is also available in other languages:** [Deutsch (German)](readme-de.md)

## Overview

HelloPi is a simple example program that demonstrates how to develop, deploy, and remotely debug .NET 10 applications for the Raspberry Pi.

## Purpose

This project serves as a reference implementation for the following scenarios:

- **Development**: Creating .NET 10 applications for Raspberry Pi
- **Deployment**: Deploying the application to the Raspberry Pi
- **Remote Debugging**: Remote access and debugging of the application from Visual Studio Code or Visual Studio 2026

## Functionality

The program controls two LEDs on the Raspberry Pi and makes them blink. This serves as a simple yet meaningful example for GPIO access and hardware control under .NET.

## Supported Development Environments

- **Visual Studio Code** with appropriate .NET extensions
- **Visual Studio 2026** with .NET 10 SDK

## Prerequisites

- Raspberry Pi (recommended: Raspberry Pi 4 or newer)
- .NET 10 SDK
- 1 LED with appropriate resistors (the onboard LED is used as the 2nd LED)
- GPIO wiring

## Getting Started

For more information on installation and configuration, please refer to the main project documentation:

- [Installation Guide - Raspberry Pi](../../wiki/installguide-raspberrypi.md)
- [Installation Guide - Linux](../../wiki/installguide-linux.md)

## Project Structure

- `Program.cs` - Main program with LED control logic
- `hellopi.csproj` - Project file for .NET 10
- `.vscode/` - Visual Studio Code configuration files
- `Properties/launchSettings.json` - Launch settings for Visual Studio Code
- `attach_vs202x.json` - Launch settings for Visual Studio 2026 (also works with 2022 and 2019)

## License

See [LICENSE](../../LICENSE) in the main project directory.
