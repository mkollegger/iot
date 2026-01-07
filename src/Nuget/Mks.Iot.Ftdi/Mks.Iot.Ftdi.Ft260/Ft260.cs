#region License

// #region License
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
// #endregion

#endregion

using System;
using System.Threading;

namespace Mks.Iot.Ftdi.Ft260;

/// <summary>
///     <para>Dotnet for FTDI Chip FT260 - HID-class USB to UART/I2C Bridge IC</para>
/// </summary>
public class Ft260 : IDisposable
{
    private readonly CancellationTokenSource _ctsClose = new CancellationTokenSource();
    private bool _disposedValue;

    /// <summary>
    ///     Dotnet for FTDI Chip FT260 - HID-class USB to UART/I2C Bridge IC
    /// </summary>
    public Ft260()
    {
#pragma warning disable CA2000 // Dispose objects before losing scope
        Create();
#pragma warning restore CA2000 // Dispose objects before losing scope

        //StartBackgroundWorker();
    }

    #region Properties

    /// <summary>
    ///     All native lib functions prepared
    /// </summary>
    public static Ft260Wrapper Ft260Base { get; private set; } = null!;

    #endregion

    /// <summary>
    ///     Singleton of native library
    /// </summary>
    /// <returns></returns>
    public static Ft260Wrapper Create()
    {
        if (Ft260Base == null!)
        {
            Ft260Base = new Ft260Wrapper();
            Ft260Base.I2CMaster_Init();
            Ft260Base.GetI2cDevices(true);
        }

        return Ft260Base;
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _ctsClose.Cancel();
                _ctsClose.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    #region Interface Implementations

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}