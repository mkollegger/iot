//using System;
//using System.Collections.Generic;
//using System.Device.I2c;
//using System.Text;

//namespace i2capp
//{
//    public class I2CScanner
//    {
//        private readonly int _busId;

//        public I2CScanner(int busId = 1)
//        {
//            _busId = busId;
//        }

//        /// <summary>
//        /// Scannt den I2C-Bus nach Geräten (ähnlich wie i2cdetect -y 1)
//        /// </summary>
//        /// <param name="startAddress">Startadresse (Standard: 0x03)</param>
//        /// <param name="endAddress">Endadresse (Standard: 0x77)</param>
//        /// <returns>Liste der gefundenen Adressen</returns>
//        public List<int> ScanBus(int startAddress = 0x03, int endAddress = 0x77)
//        {
//            var foundDevices = new List<int>();

//            Console.WriteLine($"Scanning I2C bus {_busId}...");
//            Console.WriteLine("     0  1  2  3  4  5  6  7  8  9  a  b  c  d  e  f");

//            for (int address = 0; address <= 0x7F; address++)
//            {
//                if (address % 16 == 0)
//                {
//                    Console.Write($"{address:x2}:  ");
//                }

//                // Überspringe reservierte Adressen
//                if (address < startAddress || address > endAddress)
//                {
//                    Console.Write("   ");
//                }
//                else
//                {
//                    if (ProbeAddress(address))
//                    {
//                        Console.Write($"{address:x2} ");
//                        foundDevices.Add(address);
//                    }
//                    else
//                    {
//                        Console.Write("-- ");
//                    }
//                }

//                if (address % 16 == 15)
//                {
//                    Console.WriteLine();
//                }
//            }

//            Console.WriteLine($"\nFound {foundDevices.Count} device(s)");
//            return foundDevices;
//        }

//        /// <summary>
//        /// Testet eine einzelne I2C-Adresse mit mehreren Methoden
//        /// </summary>
//        private bool ProbeAddress(int address)
//        {
//            var settings = new I2cConnectionSettings(_busId, address);

//            try
//            {
//                using (var device = I2cDevice.Create(settings))
//                {
//                    // Methode 1: Versuche zu schreiben (ähnlich wie i2cdetect mit write)
//                    // Dies ist die zuverlässigste Methode
//                    try
//                    {
//                        device.WriteByte(0x00);
//                        return true;
//                    }
//                    catch (System.IO.IOException)
//                    {
//                        // Schreiben fehlgeschlagen, versuche zu lesen
//                    }

//                    // Methode 2: Versuche zu lesen
//                    try
//                    {
//                        device.ReadByte();
//                        return true;
//                    }
//                    catch (System.IO.IOException)
//                    {
//                        // Auch Lesen fehlgeschlagen
//                    }

//                    // Methode 3: Quick Write (nur Adresse senden, kein Datenbyte)
//                    // Dies entspricht am ehesten i2cdetect -q
//                    try
//                    {
//                        // Ein leeres Write ohne Daten
//                        device.Write(ReadOnlySpan<byte>.Empty);
//                        return true;
//                    }
//                    catch (System.IO.IOException)
//                    {
//                        return false;
//                    }
//                }
//            }
//            catch (Exception)
//            {
//                return false;
//            }
//        }

//        /// <summary>
//        /// Alternative, noch genauere Probe-Methode (ähnlich i2cdetect -q)
//        /// </summary>
//        private bool ProbeAddressQuick(int address)
//        {
//            var settings = new I2cConnectionSettings(_busId, address);

//            try
//            {
//                using (var device = I2cDevice.Create(settings))
//                {
//                    // Quick command:  Nur Adresse senden, kein Register
//                    device.Write(ReadOnlySpan<byte>.Empty);
//                    return true;
//                }
//            }
//            catch (System.IO.IOException)
//            {
//                // Kein ACK erhalten
//                return false;
//            }
//            catch (Exception)
//            {
//                return false;
//            }
//        }
//    }
//}
