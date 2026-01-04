# HelloPi - Raspberry Pi LED Beispiel

## Überblick

HelloPi ist ein einfaches Beispielprogramm, das demonstriert, wie man mit .NET 10 Anwendungen für den Raspberry Pi entwickelt, deployed und remote debuggt.

## Zweck

Dieses Projekt dient als Referenzimplementierung für folgende Szenarien:

- **Entwicklung**: Erstellen von .NET 10 Anwendungen für Raspberry Pi
- **Deployment**: Bereitstellen der Anwendung auf dem Raspberry Pi
- **Remote Debugging**: Fernzugriff und Debugging der Anwendung aus Visual Studio Code oder Visual Studio 2026

## Funktionalität

Das Programm steuert zwei LEDs am Raspberry Pi und lässt diese blinken. Dies dient als einfaches, aber aussagekräftiges Beispiel für GPIO-Zugriff und Hardware-Steuerung unter .NET.

## Unterstützte Entwicklungsumgebungen

- **Visual Studio Code** mit entsprechenden .NET-Erweiterungen
- **Visual Studio 2026** mit .NET 10 SDK

## Voraussetzungen

- Raspberry Pi (empfohlen: Raspberry Pi 4 oder neuer)
- .NET 10 SDK
- 1 LEDs mit passenden Vorwiderständen (als 2te Led wird die Onboard-Led verwendet)
- GPIO-Verkabelung

## Erste Schritte

Weitere Informationen zur Installation und Konfiguration finden Sie in der Hauptdokumentation des Projekts:

- [Installation Guide - Raspberry Pi](../../wiki/installguide-raspberrypi.md)
- [Installation Guide - Linux](../../wiki/installguide-linux.md)

## Projektstruktur

- `Program.cs` - Hauptprogramm mit LED-Steuerungslogik
- `hellopi.csproj` - Projektdatei für .NET 10
- `.vscode/` - Visual Studio Code Konfigurationsdateien
- `Properties/launchSettings.json` - Launch-Einstellungen für Visual Studio Code
- `attach_vs202x.json` - Launch-Einstellungen für Visual Studio 2026 (funktioniert auch mit 2022 und 2019)

## Lizenz

Siehe [LICENSE](../../LICENSE) im Hauptverzeichnis des Projekts.
