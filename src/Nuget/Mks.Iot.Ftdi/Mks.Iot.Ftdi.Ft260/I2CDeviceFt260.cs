#region License

// MIT License
// 
// Copyright (C) 2026 Michael Kollegger
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Linq;
// ReSharper disable InconsistentNaming

namespace Mks.Iot.Ftdi.Ft260;

/// <summary>
///     The communications channel to a device on an I2C bus.
/// </summary>
public class I2cDeviceFt260 : I2cDevice
{
    private readonly Ft260Wrapper _ft260Base;
    private readonly byte _slaveAddress;

    internal I2cDeviceFt260(I2cConnectionSettings settings, Ft260Wrapper i2CFt260)
    {
        ConnectionSettings = settings;
        _ft260Base = i2CFt260;
        _slaveAddress = (byte) settings.DeviceAddress;
    }

    #region Properties

    /// <summary>
    ///     The connection settings of a device on an I2C bus. The connection settings are immutable after the device is
    ///     created
    ///     so the object returned will be a clone of the settings object.
    /// </summary>
    public override I2cConnectionSettings ConnectionSettings { get; }

    #endregion

    /// <summary>
    ///     Creates a new I2CDeviceFt260
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public new static I2cDevice Create(I2cConnectionSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return new I2cDeviceFt260(settings, Ft260Device.Create());
    }

    /// <summary>Reads a byte from the I2C device.</summary>
    /// <returns>A byte read from the I2C device.</returns>
    public override byte ReadByte()
    {
        (List<byte>? data, Ft260I2cControllerStatus status) result = _ft260Base.I2CMaster_Read(_slaveAddress, FT260_I2C_FLAG.FT260_I2C_START_AND_STOP, 1);
        if (result.data == null || result.data.Count <= 0)
        {
            return 0;
        }

        if (result.status == Ft260I2cControllerStatus.Idle)
        {
            return result.data[0];
        }

        if (result.status == Ft260I2cControllerStatus.AddressNack ||
            result.status == Ft260I2cControllerStatus.DataNack)
        {
            throw new System.IO.IOException("Data not acknowledged during last operation.");
        }

        throw new Exception("An error occurred during the I2C read operation.");
    }

    /// <summary>Reads data from the I2C device.</summary>
    /// <param name="buffer">
    ///     The buffer to read the data from the I2C device.
    ///     The length of the buffer determines how much data to read from the I2C device.
    /// </param>
    public override void Read(Span<byte> buffer)
    {
        if (buffer.Length == 0)
        {
            throw new ArgumentException($"{nameof(buffer)} cannot be empty.");
        }

        (List<byte>? data, Ft260I2cControllerStatus status) result = _ft260Base.I2CMaster_Read(_slaveAddress, FT260_I2C_FLAG.FT260_I2C_START_AND_STOP, (uint) buffer.Length);
        if (result.data == null || result.data.Count <= 0)
        {
            return;
        }

        if (result.status == Ft260I2cControllerStatus.Idle)
        {
            new Span<byte>(result.data.ToArray()).CopyTo(buffer);
            return;
        }

        if (result.status == Ft260I2cControllerStatus.AddressNack ||
            result.status == Ft260I2cControllerStatus.DataNack)
        {
            throw new System.IO.IOException("Data not acknowledged during last operation.");
        }

        throw new Exception("An error occurred during the I2C read operation.");
    }

    /// <summary>Writes a byte to the I2C device.</summary>
    /// <param name="value">The byte to be written to the I2C device.</param>
    public override void WriteByte(byte value)
    {
        var r = _ft260Base.I2CMaster_Write(_slaveAddress, FT260_I2C_FLAG.FT260_I2C_START_AND_STOP, new List<byte> {value});
        if (r.bytesWritten != 1)
        {
            throw new Exception("Not all bytes were written to the I2C device.");
        }

        if (r.status == Ft260I2cControllerStatus.Idle)
        {
            return;
        }

        if (r.status == Ft260I2cControllerStatus.AddressNack ||
            r.status == Ft260I2cControllerStatus.DataNack)
        {
            throw new System.IO.IOException("Data not acknowledged during last operation.");
        }

        throw new Exception("An error occurred during the I2C write operation.");
    }

    /// <summary>Writes data to the I2C device.</summary>
    /// <param name="buffer">
    ///     The buffer that contains the data to be written to the I2C device.
    ///     The data should not include the I2C device address.
    /// </param>
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length == 0)
        {
            throw new ArgumentException($"{nameof(buffer)} cannot be empty.");
        }
        var r = _ft260Base.I2CMaster_Write(_slaveAddress, FT260_I2C_FLAG.FT260_I2C_START_AND_STOP, buffer.ToArray().ToList());
        if (r.bytesWritten != buffer.Length)
        {
            throw new Exception("Not all bytes were written to the I2C device.");
        }

        if (r.status == Ft260I2cControllerStatus.Idle)
        {
            return;
        }

        if (r.status == Ft260I2cControllerStatus.AddressNack ||
            r.status == Ft260I2cControllerStatus.DataNack)
        {
            throw new System.IO.IOException("Data not acknowledged during last operation.");
        }

        throw new Exception("An error occurred during the I2C write operation.");
    }

    /// <summary>
    ///     Performs an atomic operation to write data to and then read data from the I2C bus on which the device is connected,
    ///     and sends a restart condition between the write and read operations.
    /// </summary>
    /// <param name="writeBuffer">
    ///     The buffer that contains the data to be written to the I2C device.
    ///     The data should not include the I2C device address.
    /// </param>
    /// <param name="readBuffer">
    ///     The buffer to read the data from the I2C device.
    ///     The length of the buffer determines how much data to read from the I2C device.
    /// </param>
    public override void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
    {
        if (writeBuffer.Length == 0)
        {
            throw new ArgumentException($"{nameof(writeBuffer)} cannot be empty.");
        }

        if (readBuffer.Length == 0)
        {
            throw new ArgumentException($"{nameof(readBuffer)} cannot be empty.");
        }

        _ft260Base.I2CMaster_Write(_slaveAddress, FT260_I2C_FLAG.FT260_I2C_START, writeBuffer.ToArray().ToList());
        (List<byte>? data, Ft260I2cControllerStatus status) result = _ft260Base.I2CMaster_Read(_slaveAddress, FT260_I2C_FLAG.FT260_I2C_STOP, (uint) readBuffer.Length);
        if (result.data == null || result.data.Count <= 0)
        {
            return;
        }

        new Span<byte>(result.data.ToArray()).CopyTo(readBuffer);
    }
}