# Mks.Common

A common utility library providing shared extension methods and helpers for .NET applications. This package currently focuses on `ILogger` extensions for safer logging and `Assembly` extensions for resource management.

## Features

### Logging Extensions
Safety wrappers for `Microsoft.Extensions.Logging.ILogger`. These methods allow you to call logging methods without checking for null. If the logger is null and a debugger is attached, messages are written to `System.Diagnostics.Debug`.

- **Safe Logging**: `TryLogInfo`, `TryLogWarning`, `TryLogError`, `TryLogTrace`, `TryLogCritical`.
- **Fallback**: Writes to debug output if no logger is configured but a debugger is attached.

```csharp
using Mks.Common.Ext;

ILogger? logger = null; // or provided via DI

// Safe to call even if logger is null
logger.TryLogInfo("Application started");
logger.TryLogError("Something went wrong");
```

### Assembly Extensions
Helper methods for working with Assembly resources.

- **GetManifestStoreToFile**: Extracts an embedded resource from an assembly and saves it to a file stream.

```csharp
using Mks.Common.Ext;
using System.Reflection;

var assembly = Assembly.GetExecutingAssembly();
using var fileStream = File.Create("output.dll");

// Extract embedded resource to file
bool success = assembly.GetManifestStoreToFile("MyProject.Resources.Lib.dll", fileStream, logger);
```

## Requirements

- **Runtime**: .NET Standard 2.1 compatible.
- **Dependencies**: `Microsoft.Extensions.Logging.Abstractions`.

## License

This project is licensed under the MIT License. See the LICENSE file for details.
