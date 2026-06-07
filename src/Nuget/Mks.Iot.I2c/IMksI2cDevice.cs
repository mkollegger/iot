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
using System.Threading.Tasks;

namespace Mks.Iot.I2c
{
    /// <summary>
    /// <para>IMksI2cDevce Interface for common functions</para>
    /// Interface IMksI2cDevice. (C) 2026 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public interface IMksI2cDevice
    {
        /// <summary>
        /// Chip Test for the device. This method should be implemented by each device to perform a self-test or check if the device is responding correctly.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task StartTest(CancellationToken cancellationToken = default);
    }
}