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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Mks.Common.Ext;

// ReSharper disable InconsistentNaming
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1028 // Enum Storage should be Int32
#pragma warning disable CA1027 // Mark enums with FlagsAttribute
#pragma warning disable CA1008 // Enums should have zero value
#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning disable CA5392 // Use DefaultDllImportSearchPaths attribute for P/Invokes

namespace Mks.Iot.Ftdi.Ft260
{
    [Flags]
    public enum Ft260I2cControllerStatus : byte
    {
        Unknown = 0x00,
        Busy = 0x01,
        Error = 0x02,
        AddressNack = 0x04,
        DataNack = 0x08,
        ArbitrationLost = 0x10,
        Idle = 0x20,
        BusBusy = 0x40
    }

    public enum Ft260GpioDir : byte
    {
        Input = 0,
        Output = 1,
    }

    #region Enums LibFT260

    public enum FT260_STATUS
    {
        FT260_OK,
        FT260_INVALID_HANDLE,
        FT260_DEVICE_NOT_FOUND,
        FT260_DEVICE_NOT_OPENED,
        FT260_DEVICE_OPEN_FAIL,
        FT260_DEVICE_CLOSE_FAIL,
        FT260_INCORRECT_INTERFACE,
        FT260_INCORRECT_CHIP_MODE,
        FT260_DEVICE_MANAGER_ERROR,
        FT260_IO_ERROR,
        FT260_INVALID_PARAMETER,
        FT260_NULL_BUFFER_POINTER,
        FT260_BUFFER_SIZE_ERROR,
        FT260_UART_SET_FAIL,
        FT260_RX_NO_DATA,
        FT260_GPIO_WRONG_DIRECTION,
        FT260_INVALID_DEVICE,
        FT260_INVALID_OPEN_DRAIN_SET,
        FT260_INVALID_OPEN_DRAIN_RESET,
        FT260_I2C_READ_FAIL,
        FT260_OTHER_ERROR
    }

    public enum FT260_GPIO2_Pin : byte
    {
        FT260_GPIO2_GPIO = 0,
        FT260_GPIO2_SUSPOUT = 1,
        FT260_GPIO2_PWREN = 2,
        FT260_GPIO2_TX_LED = 4
    }

    public enum FT260_GPIOA_Pin : byte
    {
        FT260_GPIOA_GPIO = 0,
        FT260_GPIOA_TX_ACTIVE = 3,
        FT260_GPIOA_TX_LED = 4
    }

    public enum FT260_GPIOG_Pin : byte
    {
        FT260_GPIOG_GPIO = 0,
        FT260_GPIOG_PWREN = 2,
        FT260_GPIOG_RX_LED = 5,
        FT260_GPIOG_BCD_DET = 6
    }

    public enum FT260_Clock_Rate : byte
    {
        FT260_SYS_CLK_12M = 0,
        FT260_SYS_CLK_24M,
        FT260_SYS_CLK_48M
    }

    public enum FT260_Interrupt_Trigger_Type : byte
    {
        FT260_INTR_RISING_EDGE = 0,
        FT260_INTR_LEVEL_HIGH,
        FT260_INTR_FALLING_EDGE,
        FT260_INTR_LEVEL_LOW
    }

    public enum FT260_Interrupt_Level_Time_Delay : byte
    {
        FT260_INTR_DELY_1MS = 1,
        FT260_INTR_DELY_5MS,
        FT260_INTR_DELY_30MS
    }

    public enum FT260_Suspend_Out_Polarity : byte
    {
        FT260_SUSPEND_OUT_LEVEL_HIGH = 0,
        FT260_SUSPEND_OUT_LEVEL_LOW
    }

    public enum FT260_UART_Mode : byte
    {
        FT260_UART_OFF = 0,
        FT260_UART_RTS_CTS_MODE, // hardware flow control RTS, CTS mode
        FT260_UART_DTR_DSR_MODE, // hardware flow control DTR, DSR mode
        FT260_UART_XON_XOFF_MODE, // software flow control mode
        FT260_UART_NO_FLOW_CTRL_MODE // no flow control mode
    }

    public enum FT260_Data_Bit : byte
    {
        FT260_DATA_BIT_7 = 7,
        FT260_DATA_BIT_8 = 8
    }

    public enum FT260_Stop_Bit : byte
    {
        FT260_STOP_BITS_1 = 0,
        FT260_STOP_BITS_2 = 2
    }

    public enum FT260_Parity : byte
    {
        FT260_PARITY_NONE = 0,
        FT260_PARITY_ODD,
        FT260_PARITY_EVEN,
        FT260_PARITY_MARK,
        FT260_PARITY_SPACE
    }

    public enum FT260_RI_Wakeup_Type : byte
    {
        FT260_RI_WAKEUP_RISING_EDGE = 0,
        FT260_RI_WAKEUP_FALLING_EDGE,
    }

    public enum FT260_GPIO_DIR : byte
    {
        FT260_GPIO_IN = 0,
        FT260_GPIO_OUT
    }

    public enum FT260_GPIO : ushort
    {
        FT260_GPIO_0 = 1 << 0,
        FT260_GPIO_1 = 1 << 1,
        FT260_GPIO_2 = 1 << 2,
        FT260_GPIO_3 = 1 << 3,
        FT260_GPIO_4 = 1 << 4,
        FT260_GPIO_5 = 1 << 5,
        FT260_GPIO_A = 1 << 6,
        FT260_GPIO_B = 1 << 7,
        FT260_GPIO_C = 1 << 8,
        FT260_GPIO_D = 1 << 9,
        FT260_GPIO_E = 1 << 10,
        FT260_GPIO_F = 1 << 11,
        FT260_GPIO_G = 1 << 12,
        FT260_GPIO_H = 1 << 13
    }

    public enum FT260_I2C_FLAG : byte
    {
        FT260_I2C_NONE = 0,
        FT260_I2C_START = 0x02,
        FT260_I2C_REPEATED_START = 0x03,
        FT260_I2C_STOP = 0x04,
        FT260_I2C_START_AND_STOP = 0x06
    }

    public enum FT260_PARAM_1 : byte
    {
        FT260_DS_CTL0 = 0x50,
        FT260_DS_CTL3 = 0x51,
        FT260_DS_CTL4 = 0x52,
        FT260_SR_CTL0 = 0x53,
        FT260_GPIO_PULL_UP = 0x61,
        FT260_GPIO_OPEN_DRAIN = 0x62,
        FT260_GPIO_PULL_DOWN = 0x63,
        FT260_GPIO_GPIO_SLEW_RATE = 0x65
    }

    public enum FT260_PARAM_2 : byte
    {
        FT260_GPIO_GROUP_SUSPEND_0 = 0x10, // for gpio 0 ~ gpio 5
        FT260_GPIO_GROUP_SUSPEND_A = 0x11, // for gpio A ~ gpio H
        FT260_GPIO_DRIVE_STRENGTH = 0x64
    }

    #endregion

    #region Strukturen LibFT260

    public struct FT260_GPIO_Report
    {
        public ushort value; // GPIO0~5 values
        public ushort dir; // GPIO0~5 directions
        public ushort gpioN_value; // GPIOA~H values
        public ushort gpioN_dir; // GPIOA~H directions
    }


    public struct UartConfig
    {
        public byte flow_ctrl;
        public UInt32 baud_rate;
        public byte data_bit;
        public byte parity;
        public byte stop_bit;
        public byte breaking;
    }


    public struct FT260_PinStatus
    {
        public ushort gpio_en; // GPIO0~5 pin enable
        public ushort gpio_dir; // GPIO0~5 directions
        public ushort gpio_OD_en; // GPIO0~5 open drain enable
        public ushort gpioN_en; // GPIOA~H pin enable
        public ushort gpioN_dir; // GPIOA~H directions
    }

    #endregion

    /// <summary>
    ///     <para>Wrapper for LibFT260</para>
    /// </summary>
#pragma warning disable CA1060 // Move pinvokes to native methods class
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class Ft260Wrapper : IDisposable
#pragma warning restore CA1060 // Move pinvokes to native methods class
    {
        private const string DllLocation = @"LibFT260.dll";
        private readonly uint _ft260I2C_DEVICE = 0;
        private readonly ushort _ft260PID = 0x6030;
        private readonly ushort _ft260VID = 0x0403;

        private readonly ILogger? _log;
        private bool _disposedValue;
        private IntPtr _ft260Handle = IntPtr.Zero;

        public List<byte> I2cDevices = new List<byte>();

        /// <summary>
        ///     Wrapper for LibFT260 (FTDI)
        /// </summary>
        public Ft260Wrapper(ILogger? logger = null)
        {
            _log = logger;

            // Check - only windows is supported at the moment
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _log?.TryLogError($"[{nameof(Ft260Wrapper)}]({nameof(Ft260Wrapper)}): Only Windows OS is supported at the moment!");
                throw new PlatformNotSupportedException("Only Windows OS is supported at the moment!");
            }
            
            FileInfo fi = new FileInfo(Assembly.GetEntryAssembly()!.Location);
            FileInfo nativeDll = new FileInfo(Path.Combine(fi.DirectoryName!, "LibFT260.dll"));
            if (!nativeDll.Exists || nativeDll.Length == 0)
            {
                string folder = string.Empty;

                switch (RuntimeInformation.ProcessArchitecture)
                {
                    case Architecture.Arm64:
                        folder = "arm64";
                        break;
                    case Architecture.X64:
                        folder = "amd64";
                        break;
                    case Architecture.X86:
                        folder = "ix86";
                        break;
                }

                using FileStream fileStream = File.Create(nativeDll.FullName);
                bool r = Assembly.GetExecutingAssembly().GetManifestStoreToFile($"Mks.Iot.Ftdi.Ft260.Lib.{folder}.LibFT260.dll", fileStream, _log);
                if (!r)
                {
                    _log.TryLogError($"[{nameof(Ft260Wrapper)}]({nameof(Ft260Wrapper)}): Can not store native dll!");
                    throw new Exception($"Can not store native dll for platform {folder}");
                }
            }

            Open();

            _log.TryLogInfo($"[{nameof(Ft260Wrapper)}]({nameof(Ft260Wrapper)}): Native Lib version: {GetLibVersion()}");
            _log.TryLogInfo($"[{nameof(Ft260Wrapper)}]({nameof(Ft260Wrapper)}): Chip version: {GetChipVersion()}");
        }


        /// <summary>
        ///     Which I2C devices were found on the bus
        /// </summary>
        /// <returns></returns>
        public List<byte> GetI2cDevices(bool forceUpdate = false)
        {
            if (!forceUpdate && I2cDevices.Count > 0)
            {
                return I2cDevices;
            }

            List<byte> r = new List<byte>();

            for (byte i = 8; i <= 128; i++)
            {
                (List<byte>? data, Ft260I2cControllerStatus status) check = I2CMaster_Read(i, FT260_I2C_FLAG.FT260_I2C_START_AND_STOP, 1, 10, true, true);
                if (check.status == Ft260I2cControllerStatus.Idle)
                {
                    r.Add(i);
                }
            }

            if (r.Count > 0)
            {
                string s = string.Empty;
                foreach (byte b in r)
                {
                    s += "0x" + b.ToString("X2", CultureInfo.CurrentCulture) + " ";
                }

                _log.TryLogTrace($"[{nameof(Ft260Wrapper)}]({nameof(GetI2cDevices)}): {r.Count} slave(s) at {s}");
            }

            I2cDevices = r;
            return I2cDevices;
        }

        /// <summary>
        ///     Uart - not (fully) implemented
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void UART_Init()
        {
            throw new NotImplementedException("Native functions ready prepared. But not processed. Not supported at the moment.");
        }


        /// <summary>
        ///     Check native DLL return value
        /// </summary>
        /// <param name="result"></param>
        /// <param name="noDebugLog"></param>
        /// <returns></returns>
        private bool CheckResult(FT260_STATUS result, bool noDebugLog = false)
        {
            if (result == FT260_STATUS.FT260_OK)
            {
                return true;
            }

            if (!noDebugLog)
            {
#pragma warning disable CA1873
                _log.LogError($"[{nameof(Ft260Wrapper)}]({nameof(CheckResult)}): Error result - {result}");
#pragma warning restore CA1873
            }

            return false;
        }

        /// <summary>
        ///     Check if communication is possible
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void Check()
        {
            if (_ft260Handle == IntPtr.Zero)
            {
                throw new Exception("Not connected to FT260!");
            }
        }

        #region General Functions

        /// <summary>
        ///     Number of HID Devices
        /// </summary>
        /// <returns></returns>
        public ulong CreateDeviceList()
        {
            Check();

            uint result = 0;
            if (CheckResult(FT260_CreateDeviceList(out result)))
            {
                return result;
            }

            return 0;
        }

        /// <summary>
        ///     All HID Devices
        /// </summary>
        /// <returns></returns>
        public List<string> GetDevicePath()
        {
            List<string> r = new List<string>();

            ulong lpdwNumDevs = CreateDeviceList();

            for (ulong i = 0; i < lpdwNumDevs; i++)
            {
                StringBuilder s1 = new StringBuilder(512);
                if (CheckResult(FT260_GetDevicePath(s1, 512, i)))
                {
                    r.Add(s1.ToString());
                }
            }

            return r;
        }

        /// <summary>
        ///     Connect to device
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            bool r = CheckResult(FT260_OpenByVidPid(_ft260VID, _ft260PID, _ft260I2C_DEVICE, out _ft260Handle));
            if (r)
            {
                _log.TryLogInfo($"[{nameof(Ft260Wrapper)}]({nameof(Open)}): Chip Version {GetChipVersion()}");
            }

            return r;
        }

        /// <summary>
        ///     Close connection to device
        /// </summary>
        public void Close()
        {
            if (_ft260Handle != IntPtr.Zero)
            {
                CheckResult(FT260_Close(_ft260Handle));
            }
        }

        /// <summary>
        ///     Set system clock rate. The default clock rate of the FT260 is 48 MHz.
        /// </summary>
        /// <param name="clk"></param>
        /// <returns></returns>
        public bool SetClock(FT260_Clock_Rate clk = FT260_Clock_Rate.FT260_SYS_CLK_48M)
        {
            Check();
            return CheckResult(FT260_SetClock(_ft260Handle, clk));
        }

        /// <summary>
        ///     Enable/Disable wakeup interrupt.
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        public bool SetWakeupInterrupt(bool enable)
        {
            Check();
            return CheckResult(FT260_SetWakeupInterrupt(_ft260Handle, enable));
        }

        /// <summary>
        ///     Specify edge, level and duration of signals to generate interrupt.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public bool SetInterruptTriggerType(FT260_Interrupt_Trigger_Type type, FT260_Interrupt_Level_Time_Delay delay)
        {
            Check();
            return CheckResult(FT260_SetInterruptTriggerType(_ft260Handle, type, delay));
        }

        /// <summary>
        ///     Select the function of GPIO 2.
        /// </summary>
        /// <param name="gpio2Function"></param>
        /// <returns></returns>
        public bool SelectGpio2Function(FT260_GPIO2_Pin gpio2Function)
        {
            Check();
            return CheckResult(FT260_SelectGpio2Function(_ft260Handle, gpio2Function));
        }

        /// <summary>
        ///     Select the function of GPIO A.
        /// </summary>
        /// <param name="gpioAFunction"></param>
        /// <returns></returns>
        public bool SelectGpioAFunction(FT260_GPIOA_Pin gpioAFunction)
        {
            Check();
            return CheckResult(FT260_SelectGpioAFunction(_ft260Handle, gpioAFunction));
        }

        /// <summary>
        ///     Select the function of GPIO G.
        /// </summary>
        /// <param name="gpioGFunction"></param>
        /// <returns></returns>
        public bool SelectGpioGFunction(FT260_GPIOG_Pin gpioGFunction)
        {
            Check();
            return CheckResult(FT260_SelectGpioGFunction(_ft260Handle, gpioGFunction));
        }

        /// <summary>
        ///     Set suspend out polarity.
        /// </summary>
        /// <param name="polarity"></param>
        /// <returns></returns>
        public bool SetSuspendOutPolarity(FT260_Suspend_Out_Polarity polarity)
        {
            Check();
            return CheckResult(FT260_SetSuspendOutPolarity(_ft260Handle, polarity));
        }

        /// <summary>
        ///     Version of native FT260 DLL
        /// </summary>
        /// <returns></returns>
        public string GetLibVersion()
        {
            ulong libVersion = 0;
            if (CheckResult(FT260_GetLibVersion(out libVersion)))
            {
                string r = string.Empty;
                byte[] bytes = BitConverter.GetBytes(libVersion);
                for (int i = 3; i > -1; i--)
                {
                    r += bytes[i];
                    if (i != 0)
                    {
                        r += ".";
                    }
                }

                return r;
            }

            return "?";
        }

        /// <summary>
        ///     Version of native FT260 DLL (Chip Version)
        /// </summary>
        /// <returns></returns>
        public string GetChipVersion()
        {
            Check();
            ulong chipVersion = 0;
            if (CheckResult(FT260_GetChipVersion(_ft260Handle, out chipVersion)))
            {
                string r = string.Empty;
                byte[] bytes = BitConverter.GetBytes(chipVersion);
                for (int i = 3; i > -1; i--)
                {
                    r += bytes[i];
                    if (i != 0)
                    {
                        r += ".";
                    }
                }

                return r;
            }

            return "?";
        }

        public bool EnableI2CPin(bool enable)
        {
            Check();
            return CheckResult(FT260_EnableI2CPin(_ft260Handle, enable));
        }

        public bool SetUartToGPIOPin()
        {
            Check();
            return CheckResult(FT260_SetUartToGPIOPin(_ft260Handle));
        }

        public bool FT260_SetGPIOToUartPin()
        {
            Check();
            return CheckResult(FT260_SetGPIOToUartPin(_ft260Handle));
        }


        public bool EnableDcdRiPin(bool enable)
        {
            Check();
            return CheckResult(FT260_EnableDcdRiPin(_ft260Handle, enable));
        }

        #endregion

        #region I2C Functions

        /// <summary>
        ///     Initialize the FT260 as an I2C master with the requested I2C clock speed.
        /// </summary>
        /// <param name="kbps">The speed of the I2C clock, whose range is from 100K bps to 4000K bps.</param>
        /// <returns></returns>
        public bool I2CMaster_Init(UInt32 kbps = 400)
        {
            Check();
            EnableI2CPin(true);
            bool r = CheckResult(FT260_I2CMaster_Init(_ft260Handle, kbps));
            //I2CMaster_Reset();
            return r;
        }

        /// <summary>
        ///     Reset the FT260 I2C Master controller.
        /// </summary>
        /// <returns></returns>
        public bool I2CMaster_Reset()
        {
            Check();
            return CheckResult(FT260_I2CMaster_Reset(_ft260Handle));
        }

        /// <summary>
        ///     Read the status of the I2C master controller.
        /// </summary>
        /// <param name="noDebugLog">No debug output</param>
        /// <param name="scan">"quick" scan for slaves</param>
        /// <returns></returns>
        public Ft260I2cControllerStatus I2CMaster_GetStatus(bool noDebugLog = false, bool scan = false)
        {
            Check();
            byte status = 0;
            if (CheckResult(FT260_I2CMaster_GetStatus(_ft260Handle, out status)))
            {
                Ft260I2cControllerStatus s = (Ft260I2cControllerStatus) status;
                if (!scan)
                {
                    if (s != Ft260I2cControllerStatus.Idle)
                    {
                        int i = 0;
                        do
                        {
                            Thread.Sleep(50);
                            CheckResult(FT260_I2CMaster_GetStatus(_ft260Handle, out status));
                            s = (Ft260I2cControllerStatus) status;
                            if (s == Ft260I2cControllerStatus.Idle)
                            {
                                break;
                            }

                            i++;
                        } while (i < 10);
                    }
                }

                if (noDebugLog)
                {
                    return s;
                }

                if (status != 0)
                {
                    if (s.HasFlag(Ft260I2cControllerStatus.Busy))
                    {
                        _log.LogError($"[{nameof(Ft260Wrapper)}]({nameof(I2CMaster_GetStatus)}): Controller busy");
                    }
                    else
                    {
                        if (s.HasFlag(Ft260I2cControllerStatus.Error))
                        {
                            _log.LogError($"[{nameof(Ft260Wrapper)}]({nameof(I2CMaster_GetStatus)}): Error condition");
                        }

                        if (s.HasFlag(Ft260I2cControllerStatus.AddressNack))
                        {
                            _log.LogError($"[{nameof(Ft260Wrapper)}]({nameof(I2CMaster_GetStatus)}): Slave address was not acknowledged during last operation");
                        }

                        if (s.HasFlag(Ft260I2cControllerStatus.DataNack))
                        {
                            _log.LogError($"[{nameof(Ft260Wrapper)}]({nameof(I2CMaster_GetStatus)}): Data not acknowledged during last operation");
                        }

                        if (s.HasFlag(Ft260I2cControllerStatus.ArbitrationLost))
                        {
                            _log.LogError($"[{nameof(Ft260Wrapper)}]({nameof(I2CMaster_GetStatus)}): Arbitration lost during last operation");
                        }

                        if (s.HasFlag(Ft260I2cControllerStatus.Idle))
                        {
                            //_log.TryLogTrace($"[{nameof(Ft260Wrapper)}]({nameof(I2CMaster_GetStatus)}): Controller idle");
                        }

                        if (s.HasFlag(Ft260I2cControllerStatus.BusBusy))
                        {
                            _log.LogWarning($"[{nameof(Ft260Wrapper)}]({nameof(I2CMaster_GetStatus)}): Bus busy");
                        }
                    }
                }

                return (Ft260I2cControllerStatus) status;
            }

            return Ft260I2cControllerStatus.Unknown;
        }


        /// <summary>
        ///     Read data from the specified I2C slave device with the given I2C condition.
        /// </summary>
        /// <param name="deviceAddress">Address of the target I2C slave device.</param>
        /// <param name="flag">I2C condition</param>
        /// <param name="bytesToRead">Number of bytes to read from the device.</param>
        /// <param name="wait_timer">
        ///     A timer counter to check I2C read status from the device If times up, the I2C will return an
        ///     error message FT260_I2C_READ_FAIL The default value is 5000(5 sec)
        /// </param>
        /// <param name="noDebugLog">No debug output</param>
        /// <param name="scan">"quick" scan for slaves</param>
        /// <returns></returns>
        public (List<byte>? data, Ft260I2cControllerStatus status) I2CMaster_Read(byte deviceAddress, FT260_I2C_FLAG flag, uint bytesToRead, ulong wait_timer = 5000, bool noDebugLog = false, bool scan = false)
        {
            Check();

            Ft260I2cControllerStatus status = I2CMaster_GetStatus(true, scan);
            if (status.HasFlag(Ft260I2cControllerStatus.Error))
            {
                I2CMaster_Reset();
            }

            byte[] buffer = new byte[bytesToRead];
            uint bytesRead = 0;
            int size = (int) (Marshal.SizeOf(buffer[0]) * bytesToRead);
            IntPtr pntr = Marshal.AllocHGlobal(size);

            try
            {
                if (CheckResult(FT260_I2CMaster_Read(_ft260Handle, deviceAddress, flag, pntr, bytesToRead, out bytesRead, wait_timer), true))
                {
                    byte[] resultBuffer = new byte[bytesRead];
                    Marshal.Copy(pntr, resultBuffer, 0, resultBuffer.Length);
                    Marshal.FreeHGlobal(pntr);
                    status = I2CMaster_GetStatus(noDebugLog, scan);
                    return (resultBuffer.ToList(), status);
                }
            }
            catch
            {
                I2CMaster_Reset();
            }

            Marshal.FreeHGlobal(pntr);
            return (null, Ft260I2cControllerStatus.Unknown);
        }

        /// <summary>
        ///     Write data to the specified I2C slave device with the given I2C condition.
        /// </summary>
        /// <param name="deviceAddress">Address of the target I2C slave.</param>
        /// <param name="flag">I2C condition</param>
        /// <param name="data">Data</param>
        /// <returns></returns>
        public (int bytesWritten, Ft260I2cControllerStatus status) I2CMaster_Write(byte deviceAddress, FT260_I2C_FLAG flag, List<byte> data)
        {
            if (data == null! || data.Count <= 0)
            {
                return (-1, Ft260I2cControllerStatus.Unknown);
            }

            Check();

            Ft260I2cControllerStatus status = I2CMaster_GetStatus(true);
            if (status != Ft260I2cControllerStatus.Idle)
            {
                I2CMaster_Reset();
            }
            //if (status.HasFlag(Ft260I2cControllerStatus.Error))
            //{
            //    I2CMaster_Reset();
            //}

            byte[] buffer = data.ToArray();
            uint bytesToWrite = (uint) data.Count;
            uint bytesWritten = 0;
            int size = (int) (Marshal.SizeOf(buffer[0]) * bytesToWrite);
            IntPtr pntr = Marshal.AllocHGlobal(size);

            Marshal.Copy(buffer, 0, pntr, (int) bytesToWrite);

            if (CheckResult(FT260_I2CMaster_Write(_ft260Handle, deviceAddress, flag, pntr, bytesToWrite, out bytesWritten)))
            {
                status = I2CMaster_GetStatus();
                Marshal.FreeHGlobal(pntr);
                return ((int) bytesWritten, status);
            }

            Marshal.FreeHGlobal(pntr);
            return (-1, Ft260I2cControllerStatus.Unknown);
        }

        #endregion

        #region GPIO Functions

        /// <summary>
        ///     Set direction for the specified GPIO pin.
        /// </summary>
        /// <returns></returns>
        public bool Gpio_SetDir(FT260_GPIO pin, Ft260GpioDir dir)
        {
            Check();
            return CheckResult(FT260_GPIO_SetDir(_ft260Handle, (byte) pin, (byte) dir));
        }

        /// <summary>
        ///     Read the value from the specified GPIO pin.
        /// </summary>
        /// <returns></returns>
        public bool? Gpio_Read(FT260_GPIO pin)
        {
            Check();
            GpioCheckConfig(pin);
            byte read = 255;
            bool r = CheckResult(FT260_GPIO_Read(_ft260Handle, (byte) pin, out read));

            if (!r)
            {
                return null;
            }

            return read != 0;
        }

        /// <summary>
        ///     Set direction for the specified GPIO pin.
        /// </summary>
        /// <returns></returns>
        public bool Gpio_Write(FT260_GPIO pin, bool value)
        {
            Check();
            GpioCheckConfig(pin);
            byte v = (value) ? (byte) 1 : (byte) 0;
            return CheckResult(FT260_GPIO_Write(_ft260Handle, (byte) pin, v));
        }

        private readonly List<FT260_GPIO> _pinsConfiguredAsGpio = new List<FT260_GPIO>();

        /// <summary>
        ///     Check if a pin is configured as GPIO
        /// </summary>
        /// <param name="pin">Pin</param>
        /// <param name="asGpio">Configure as Gpio (or disable)</param>
        public void GpioCheckConfig(FT260_GPIO pin, bool asGpio = true)
        {
            if (asGpio)
            {
                if (_pinsConfiguredAsGpio.Contains(pin))
                {
                    return;
                }

                switch (pin)
                {
                    case FT260_GPIO.FT260_GPIO_0:
                    case FT260_GPIO.FT260_GPIO_1:
                        FT260_EnableI2CPin(_ft260Handle, false);
                        break;
                    case FT260_GPIO.FT260_GPIO_2:
                        FT260_SelectGpio2Function(_ft260Handle, FT260_GPIO2_Pin.FT260_GPIO2_GPIO);
                        break;
                    case FT260_GPIO.FT260_GPIO_3:
                        FT260_SetWakeupInterrupt(_ft260Handle, false);
                        break;
                    case FT260_GPIO.FT260_GPIO_4:
                    case FT260_GPIO.FT260_GPIO_5:
                        FT260_EnableDcdRiPin(_ft260Handle, false);
                        break;
                    case FT260_GPIO.FT260_GPIO_A:
                        FT260_SelectGpioAFunction(_ft260Handle, FT260_GPIOA_Pin.FT260_GPIOA_GPIO);
                        break;
                    case FT260_GPIO.FT260_GPIO_B:
                    case FT260_GPIO.FT260_GPIO_C:
                    case FT260_GPIO.FT260_GPIO_D:
                    case FT260_GPIO.FT260_GPIO_E:
                    case FT260_GPIO.FT260_GPIO_F:
                    case FT260_GPIO.FT260_GPIO_H:
                        FT260_SetUartToGPIOPin(_ft260Handle);
                        break;
                    case FT260_GPIO.FT260_GPIO_G:
                        FT260_SelectGpioGFunction(_ft260Handle, FT260_GPIOG_Pin.FT260_GPIOG_GPIO);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(pin), pin, null);
                }

                _pinsConfiguredAsGpio.Add(pin);
            }
            else
            {
                if (!_pinsConfiguredAsGpio.Contains(pin))
                {
                    return;
                }

                switch (pin)
                {
                    case FT260_GPIO.FT260_GPIO_0:
                    case FT260_GPIO.FT260_GPIO_1:
                        FT260_EnableI2CPin(_ft260Handle, true);
                        break;
                    case FT260_GPIO.FT260_GPIO_2:
                        //Set default (from datasheet)
                        FT260_SelectGpio2Function(_ft260Handle, FT260_GPIO2_Pin.FT260_GPIO2_SUSPOUT);
                        break;
                    case FT260_GPIO.FT260_GPIO_3:
                        FT260_SetWakeupInterrupt(_ft260Handle, true);
                        break;
                    case FT260_GPIO.FT260_GPIO_4:
                    case FT260_GPIO.FT260_GPIO_5:
                        FT260_EnableDcdRiPin(_ft260Handle, true);
                        break;
                    case FT260_GPIO.FT260_GPIO_A:
                        //Set default (from datasheet)
                        FT260_SelectGpioAFunction(_ft260Handle, FT260_GPIOA_Pin.FT260_GPIOA_TX_ACTIVE);
                        break;
                    case FT260_GPIO.FT260_GPIO_B:
                    case FT260_GPIO.FT260_GPIO_C:
                    case FT260_GPIO.FT260_GPIO_D:
                    case FT260_GPIO.FT260_GPIO_E:
                    case FT260_GPIO.FT260_GPIO_F:
                    case FT260_GPIO.FT260_GPIO_H:
                        FT260_SetGPIOToUartPin(_ft260Handle);
                        break;
                    case FT260_GPIO.FT260_GPIO_G:
                        //Set default (from datasheet)
                        FT260_SelectGpioGFunction(_ft260Handle, FT260_GPIOG_Pin.FT260_GPIOG_BCD_DET);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(pin), pin, null);
                }

                _pinsConfiguredAsGpio.Remove(pin);
            }
        }


        /// <summary>
        ///     Set directions and values for all GPIO pins with the FT260_GPIO_Report parameter.
        /// </summary>
        /// <returns></returns>
        public bool Gpio_Set(FT260_GPIO_Report report)
        {
            Check();
            return CheckResult(FT260_GPIO_Set(_ft260Handle, report));
        }

        /// <summary>
        ///     Get directions and values for all GPIO pins with the FT260_GPIO_Report parameter.
        /// </summary>
        /// <returns></returns>
        public FT260_GPIO_Report? Gpio_Get()
        {
            Check();
            bool r = CheckResult(FT260_GPIO_Get(_ft260Handle, out FT260_GPIO_Report report));
            if (!r)
            {
                return null;
            }

            return report;
        }


        /// <summary>
        ///     Function to enable open drain feature. The pins for operation are defined with the parameter pinNum.
        ///     It is important to note that once this feature is enabled, the GPIO_Reset_OD must be called when the pin is to be
        ///     configured for other purposes.
        /// </summary>
        /// <returns></returns>
        public bool Gpio_Set_OD(FT260_GPIO pin)
        {
            Check();
            return CheckResult(FT260_GPIO_Set_OD(_ft260Handle, (byte) pin));
        }

        /// <summary>
        ///     To RESET open drain function
        /// </summary>
        /// <returns></returns>
        public bool Gpio_Reset_OD()
        {
            Check();
            return CheckResult(FT260_GPIO_Reset_OD(_ft260Handle));
        }

        #endregion

        #region Funktionen aus LibFT260

        #region FT260 General Functions

        [DllImport(DllLocation, EntryPoint = "FT260_CreateDeviceList", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_CreateDeviceList(out uint lpdwNumDevs);

        [DllImport(DllLocation, EntryPoint = "FT260_GetDevicePath", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_GetDevicePath([MarshalAs(UnmanagedType.LPWStr)] StringBuilder pDevicePath, ulong bufferLength, ulong deviceIndex);

        [DllImport(DllLocation, EntryPoint = "FT260_Open", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_Open(int iDevice, out IntPtr pFt260Handle);

        [DllImport(DllLocation, EntryPoint = "FT260_OpenByVidPid", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_OpenByVidPid(ushort vid, ushort pid, uint deviceIndex, out IntPtr pFt260Handle);

        [DllImport(DllLocation, EntryPoint = "FT260_OpenBySerialNumber", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_OpenBySerialNumber(string pSerialNumber, ulong deviceIndex, out IntPtr pFt260Handle);

        [DllImport(DllLocation, EntryPoint = "FT260_OpenByProductDescription", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_OpenByProductDescription(string pDescription, ulong deviceIndex, out IntPtr pFt260Handle);

        [DllImport(DllLocation, EntryPoint = "FT260_OpenByDevicePath", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_OpenByDevicePath([MarshalAs(UnmanagedType.LPWStr)] string pDevicePath, out IntPtr pFt260Handle);

        [DllImport(DllLocation, EntryPoint = "FT260_Close", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_Close(IntPtr ft260Handle);

        [DllImport(DllLocation, EntryPoint = "FT260_SetClock", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_SetClock(IntPtr ft260Handle, FT260_Clock_Rate clk);

        [DllImport(DllLocation, EntryPoint = "FT260_SetWakeupInterrupt", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_SetWakeupInterrupt(IntPtr ft260Handle, bool enable);

        [DllImport(DllLocation, EntryPoint = "FT260_SetInterruptTriggerType", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_SetInterruptTriggerType(IntPtr ft260Handle, FT260_Interrupt_Trigger_Type type, FT260_Interrupt_Level_Time_Delay delay);

        [DllImport(DllLocation, EntryPoint = "FT260_SelectGpio2Function", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_SelectGpio2Function(IntPtr ft260Handle, FT260_GPIO2_Pin gpio2Function);

        [DllImport(DllLocation, EntryPoint = "FT260_SelectGpioAFunction", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_SelectGpioAFunction(IntPtr ft260Handle, FT260_GPIOA_Pin gpioAFunction);

        [DllImport(DllLocation, EntryPoint = "FT260_SelectGpioGFunction", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_SelectGpioGFunction(IntPtr ft260Handle, FT260_GPIOG_Pin gpioGFunction);

        [DllImport(DllLocation, EntryPoint = "FT260_SetSuspendOutPolarity", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_SetSuspendOutPolarity(IntPtr ft260Handle, FT260_Suspend_Out_Polarity polarity);

        [DllImport(DllLocation, EntryPoint = "FT260_SetParam_U8", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_SetParam_U8(IntPtr ft260Handle, FT260_PARAM_1 param, byte value);

        [DllImport(DllLocation, EntryPoint = "FT260_SetParam_U16", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_SetParam_U16(IntPtr ft260Handle, FT260_PARAM_2 param, UInt16 value);

        [DllImport(DllLocation, EntryPoint = "FT260_GetChipVersion", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_GetChipVersion(IntPtr ft260Handle, out ulong lpdwChipVersion);

        [DllImport(DllLocation, EntryPoint = "FT260_GetLibVersion", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_GetLibVersion(out ulong lpdwLibVersion);

        [DllImport(DllLocation, EntryPoint = "FT260_EnableI2CPin", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_EnableI2CPin(IntPtr ft260Handle, bool enable);

        [DllImport(DllLocation, EntryPoint = "FT260_SetUartToGPIOPin", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_SetUartToGPIOPin(IntPtr ft260Handle);

        [DllImport(DllLocation, EntryPoint = "FT260_SetGPIOToUartPin", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_SetGPIOToUartPin(IntPtr handle);

        [DllImport(DllLocation, EntryPoint = "FT260_EnableDcdRiPin", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_EnableDcdRiPin(IntPtr ft260Handle, bool enable);

        #endregion

        #region FT260 I2C Functions

        [DllImport(DllLocation, EntryPoint = "FT260_I2CMaster_Init", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_I2CMaster_Init(IntPtr ft260Handle, UInt32 kbps);

        [DllImport(DllLocation, EntryPoint = "FT260_I2CMaster_Read", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_I2CMaster_Read(IntPtr handle, byte deviceAddress, FT260_I2C_FLAG flag, IntPtr lpBuffer, uint dwBytesToRead, out uint lpdwBytesReturned, ulong wait_timer = 5000);

        [DllImport(DllLocation, EntryPoint = "FT260_I2CMaster_Write", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_I2CMaster_Write(IntPtr ft260Handle, byte deviceAddress, FT260_I2C_FLAG flag, IntPtr lpBuffer, uint dwBytesToWrite, out uint lpdwBytesWritten);

        [DllImport(DllLocation, EntryPoint = "FT260_I2CMaster_GetStatus", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_I2CMaster_GetStatus(IntPtr ft260Handle, out byte status);

        [DllImport(DllLocation, EntryPoint = "FT260_I2CMaster_Reset", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_I2CMaster_Reset(IntPtr ft260Handle);

        #endregion

        #region FT260 UART Functions

        [DllImport(DllLocation, EntryPoint = "FT260_UART_Init", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_UART_Init(IntPtr ft260Handle);


        [DllImport(DllLocation, EntryPoint = "FT260_UART_SetBaudRate", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_UART_SetBaudRate(IntPtr ft260Handle, ulong baudRate);

        [DllImport(DllLocation, EntryPoint = "FT260_UART_SetFlowControl", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_UART_SetFlowControl(IntPtr ft260Handle, FT260_UART_Mode flowControl);

        [DllImport(DllLocation, EntryPoint = "FT260_UART_SetDataCharacteristics", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_UART_SetDataCharacteristics(IntPtr ft260Handle, FT260_Data_Bit dataBits, FT260_Stop_Bit stopBits, FT260_Parity parity);

        [DllImport(DllLocation, EntryPoint = "FT260_UART_SetBreakOn", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_UART_SetBreakOn(IntPtr ft260Handle);

        [DllImport(DllLocation, EntryPoint = "FT260_UART_SetBreakOff", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_UART_SetBreakOff(IntPtr ft260Handle);

        [DllImport(DllLocation, EntryPoint = "FT260_UART_SetXonXoffChar", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_UART_SetXonXoffChar(IntPtr ft260Handle, byte Xon, byte Xoff);

        [DllImport(DllLocation, EntryPoint = "FT260_UART_GetConfig", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_UART_GetConfig(IntPtr ft260Handle, out UartConfig pUartConfig);

        [DllImport(DllLocation, EntryPoint = "FT260_UART_GetQueueStatus", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_UART_GetQueueStatus(IntPtr ft260Handle, out uint lpdwAmountInRxQueue);

        [DllImport(DllLocation, EntryPoint = "FT260_UART_Read", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_UART_Read(IntPtr ft260Handle, IntPtr lpBuffer, uint dwBufferLength, uint dwBytesToRead, out uint lpdwBytesReturned);

        [DllImport(DllLocation, EntryPoint = "FT260_UART_Write", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_UART_Write(IntPtr ft260Handle, IntPtr lpBuffer, uint dwBufferLength, uint dwBytesToWrite, out uint lpdwBytesWritten);

        [DllImport(DllLocation, EntryPoint = "FT260_UART_Reset", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_UART_Reset(IntPtr ft260Handle);

        [DllImport(DllLocation, EntryPoint = "FT260_UART_GetDcdRiStatus", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_UART_GetDcdRiStatus(IntPtr ft260Handle, out byte value);

        [DllImport(DllLocation, EntryPoint = "FT260_UART_EnableRiWakeup", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_UART_EnableRiWakeup(IntPtr ft260Handle, bool enable);

        [DllImport(DllLocation, EntryPoint = "FT260_UART_SetRiWakeupConfig", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_UART_SetRiWakeupConfig(IntPtr ft260Handle, FT260_RI_Wakeup_Type type);

        #endregion

        #region FT260 Interrupt is transmitted by UART interface

        [DllImport(DllLocation, EntryPoint = "FT260_GetInterruptFlag", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_GetInterruptFlag(IntPtr ft260Handle, out bool pbFlag);

        [DllImport(DllLocation, EntryPoint = "FT260_CleanInterruptFlag", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_CleanInterruptFlag(IntPtr ft260Handle, out bool pbFlag);

        #endregion

        #region FT260 GPIO Functions

        [DllImport(DllLocation, EntryPoint = "FT260_GPIO_Set", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_GPIO_Set(IntPtr ft260Handle, FT260_GPIO_Report report);

        [DllImport(DllLocation, EntryPoint = "FT260_GPIO_Get", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_GPIO_Get(IntPtr ft260Handle, out FT260_GPIO_Report report);

        [DllImport(DllLocation, EntryPoint = "FT260_GPIO_SetDir", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_GPIO_SetDir(IntPtr ft260Handle, ushort pinNum, byte dir);

        [DllImport(DllLocation, EntryPoint = "FT260_GPIO_Read", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_GPIO_Read(IntPtr ft260Handle, ushort pinNum, out byte pValue);

        [DllImport(DllLocation, EntryPoint = "FT260_GPIO_Write", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_GPIO_Write(IntPtr ft260Handle, ushort pinNum, byte value);

        #endregion

        #region FT260 GPIO open drain

        [DllImport(DllLocation, EntryPoint = "FT260_GPIO_Set_OD", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_GPIO_Set_OD(IntPtr handle, byte pins);

        [DllImport(DllLocation, EntryPoint = "FT260_GPIO_Reset_OD", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT260_STATUS FT260_GPIO_Reset_OD(IntPtr handle);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                Close();
                _disposedValue = true;
            }
        }


        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #endregion
    }
}

#pragma warning restore CA5392 // Use DefaultDllImportSearchPaths attribute for P/Invokes
#pragma warning restore CA1028 // Enum Storage should be Int32
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CA1027 // Mark enums with FlagsAttribute
#pragma warning restore CA1008 // Enums should have zero value
#pragma warning restore CA1815 // Override equals and operator equals on value types