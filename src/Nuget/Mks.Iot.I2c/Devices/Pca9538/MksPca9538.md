# MksPca9538 - 8-bit I/O Expander

## Kurze Beschreibung zum Chip
Der PCA9538 ist ein 8-Bit I/O-Expander für den I2C-Bus mit Interrupt-Ausgang und Reset. Er ermöglicht die Erweiterung von Mikrocontroller-Systemen um zusätzliche generische Ein- und Ausgabepins (GPIOs). Der Chip zeichnet sich durch seinen geringen Stromverbrauch und die Fähigkeit aus, Zustandsänderungen an den Eingängen über einen Interrupt-Pin zu melden, wodurch Polling vermieden werden kann.

## Links zum Datenblatt
* [NXP PCA9538 Datenblatt](https://www.nxp.com/docs/en/data-sheet/PCA9538.pdf)

## Funktionen für C# Entwickler (Klasse MksPca9538)
Die Klasse `MksPca9538` ermöglicht den Zugriff auf den Chip über den I2C-Bus. Sie stellt Methoden bereit, um die internen Register direkt zu lesen und zu schreiben, sowie eine High-Level-Integration über `GpioController`.

### Register Übersicht
Der Chip verfügt über vier interne Register, die durch die Enumeration `MksPca9538Register` abgebildet werden:

1.  **Konfiguration (`Configuration`)** - Adr 0x03
    *   Steuert die Richtung der I/O-Pins.
    *   `1` = Eingang (High-Z).
    *   `0` = Ausgang.

2.  **Ausgabe (`OutputPort`)** - Adr 0x01
    *   Setzt den logischen Pegel für Pins, die als Ausgang konfiguriert sind.

3.  **Eingabe (`InputPort`)** - Adr 0x00
    *   Liest den aktuellen Zustand der Pins (unabhängig von der Konfiguration).

4.  **Polaritätsumkehr (`PolarityInversion`)** - Adr 0x02
    *   Invertiert die Polarität des Eingangsregisters.

### Verwendung als GpioController
Die Klasse bietet die Methode `GetGpioController()`, die einen Standard-.NET `GpioController` zurückgibt. Damit können die Pins wie gewohnt verwendet werden:

*   `OpenPin(pin, PinMode.Input)`
*   `Write(pin, PinValue.High)`
*   `Read(pin)`
*   Events für Zustandsänderungen (Rising/Falling).

### Überwachung (Polling & Interrupts)
Um Eingangsänderungen zu erkennen, bietet die Klasse zwei Modi:

*   **Polling:** `EnablePolling(int intervalMs)`
    *   Fragt den Chip periodisch ab.
*   **Interrupt:** `EnableInterrupt(GpioController hostGpio, int pin)`
    *   Nutzt den Hardware-Interrupt-Pin des PCA9538.
    *   Erfordert einen Host-GPIO, der mit dem INT-Pin des Chips verbunden ist.
