# Hello Debugger

> **This document is also available in other languages:** [Deutsch (German)](readme-de.md)

## Overview

HelloDebugger is a minimal .NET 10 sample for testing SSH-based remote debugging from Visual Studio 2026 with the SSH Remote Attach workflow.

## Purpose

This project serves as a reference implementation for the following scenarios:

- **Remote Debugging**: Start and debug a .NET app on a remote machine over SSH
- **Debugger Wait Mode**: Use `--debug` so the app waits for the debugger to attach
- **Launch Profile Usage**: Reuse `attach_vs202x.json` and `attach_mac.json` for remote launch settings

## Functionality

- Prints a startup message
- Runs normally without debugger when started without `--debug`
- Waits for debugger attach when started with `--debug`
- Stops at `Debugger.Break()` once attached


## Supported Development Environments

- **Visual Studio 2026** with .NET 10 SDK

## Prerequisites

- .NET 10 SDK
- Visual Studio 2026
- SSH access to your target system (Linux/macOS/Raspberry Pi)
- Remote debugger (`vsdbg`) installed on the target machine

## Getting Started

This sample follows the same approach as the SSH remote debugging tutorial:

- Tutorial: <https://github.com/mkollegger/iot/wiki/Tutorials-SshRemoteDebugVs>
- Setup: <https://github.com/mkollegger/iot/wiki/setup-sshremotedbg>

Recommended workflow:

1. Open `samples/hellodebugger` in Visual Studio 2026.
2. Build the project for your remote target runtime.
3. Copy the build output to the remote machine.
4. Adjust one of the launch files for your host/user/path:
   - `attach_vs202x.json`
   - `attach_mac.json`
5. Start the debug adapter with the selected launch file.
6. Launch with `--debug` and attach remotely.

## Project Structure

- `Program.cs` - sample app with optional debugger wait mode
- `attach_vs202x.json` - SSH launch profile example (Windows host)
- `attach_mac.json` - SSH launch profile example (macOS target)
- `readme-de.md` - German documentation

## License

See [LICENSE](../../LICENSE) in the main project directory.
