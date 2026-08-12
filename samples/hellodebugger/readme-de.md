# Hello Debugger

> **Dieses Dokument ist auch in anderen Sprachen verfügbar:** [English](README.md)

## Überblick

HelloDebugger ist ein minimales .NET-10-Beispiel für SSH-basiertes Remote-Debugging aus Visual Studio 2026 mit dem SSH Remote Attach-Workflow.

## Zweck

Dieses Projekt dient als Referenz für folgende Szenarien:

- **Remote-Debugging**: .NET-Anwendung per SSH auf einem Zielsystem starten und debuggen
- **Debugger-Wartemodus**: Mit `--debug` auf das Anhängen des Debuggers warten
- **Launch-Profile nutzen**: `attach_vs202x.json` und `attach_mac.json` als Vorlage verwenden

## Funktionalität

- Gibt eine Startmeldung aus
- Läuft ohne Debugger normal, wenn ohne `--debug` gestartet
- Wartet mit `--debug` auf das Anhängen des Debuggers
- Stoppt nach erfolgreichem Attach bei `Debugger.Break()`

## Unterstützte Entwicklungsumgebung

- **Visual Studio 2026** mit .NET 10 SDK

## Voraussetzungen

- .NET 10 SDK
- Visual Studio 2026
- SSH-Zugriff auf das Zielsystem (Linux/macOS/Raspberry Pi)
- Installierter Remote-Debugger (`vsdbg`) auf dem Zielsystem

## Erste Schritte

Das Beispiel folgt dem gleichen Ablauf wie das SSH-Remote-Debugging-Tutorial:

- Tutorial: <https://github.com/mkollegger/iot/wiki/Tutorials-SshRemoteDebugVs>
- Setup: <https://github.com/mkollegger/iot/wiki/setup-sshremotedbg>

Empfohlener Ablauf:

1. `samples/hellodebugger` in Visual Studio 2026 öffnen.
2. Projekt für die Ziel-Runtime bauen.
3. Build-Ausgabe auf das Zielsystem kopieren.
4. Eine Launch-Datei auf Host/Benutzer/Pfade anpassen:
   - `attach_vs202x.json`
   - `attach_mac.json`
5. Debug-Adapter mit der gewählten Launch-Datei starten.
6. Mit `--debug` starten und remote anhängen.

## Projektstruktur

- `Program.cs` - Beispielanwendung mit optionalem Debugger-Wartemodus
- `attach_vs202x.json` - SSH-Launch-Profil (Windows-Host)
- `attach_mac.json` - SSH-Launch-Profil (macOS-Ziel)
- `README.md` - Englische Dokumentation

## Lizenz

Siehe [LICENSE](../../LICENSE) im Hauptverzeichnis.
