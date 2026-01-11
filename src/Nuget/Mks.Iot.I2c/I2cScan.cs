using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Runtime.CompilerServices;

namespace Mks.Iot.I2c;

public class I2cScan
{
    private readonly Func<I2cConnectionSettings, I2cDevice> _createFunc;
    private readonly int _busId;

    public I2cScan(Func<I2cConnectionSettings,I2cDevice> createFunc, int busId = 1)
    {
        ArgumentNullException.ThrowIfNull(createFunc);
        
        _createFunc = createFunc;
        _busId = busId;
    }

    /// <summary>
    /// Scannt den I2C-Bus nach Geräten (ähnlich wie i2cdetect -y 1)
    /// </summary>
    /// <param name="startAddress">Startadresse (Standard: 0x03)</param>
    /// <param name="endAddress">Endadresse (Standard: 0x77)</param>
    /// <returns>Liste der gefundenen Adressen</returns>
    public List<byte> ScanBus(byte startAddress = 0x03, byte endAddress = 0x77)
    {
        var foundDevices = new List<byte>();

        Console.WriteLine($"Scanning I2C bus {_busId}...");
        Console.WriteLine("     0  1  2  3  4  5  6  7  8  9  a  b  c  d  e  f");

        for (byte address = 0; address <= 0x7F; address++)
        {
            if (address % 16 == 0)
            {
                Console.Write($"{address:x2}:  ");
            }

            // Überspringe reservierte Adressen
            if (address < startAddress || address > endAddress)
            {
                Console.Write("   ");
            }
            else
            {
                if (ProbeAddress(address))
                {
                    Console.Write($"{address:x2} ");
                    foundDevices.Add(address);
                }
                else
                {
                    Console.Write("-- ");
                }
            }

            if (address % 16 == 15)
            {
                Console.WriteLine();
            }
        }

        Console.WriteLine($"\nFound {foundDevices.Count} device(s)");
        return foundDevices;
    }

    /// <summary>
    /// Testet eine einzelne I2C-Adresse mit mehreren Methoden
    /// </summary>
    private bool ProbeAddress(byte address)
    {
        //address = 0x70;
        var settings = new I2cConnectionSettings(_busId, address);
        
        try
        {
            using var device = _createFunc(settings);
            // Methode 1: Versuche zu schreiben (ähnlich wie i2cdetect mit write)
            // Dies ist die zuverlässigste Methode
            try
            {
                device.WriteByte(0x00);
                return true;
            }
            catch (System.IO.IOException)
            {
                // Schreiben fehlgeschlagen, versuche zu lesen
            }

            // Methode 2: Versuche zu lesen
            try
            {
                device.ReadByte();
                return true;
            }
            catch (System.IO.IOException)
            {
                // Auch Lesen fehlgeschlagen
            }

            // Methode 3: Quick Write (nur Adresse senden, kein Datenbyte)
            // Dies entspricht am ehesten i2cdetect -q
            try
            {
                // Ein leeres Write ohne Daten
                device.Write(ReadOnlySpan<byte>.Empty);
                return true;
            }
            catch (System.IO.IOException)
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    
}