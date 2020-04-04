namespace Meadow.Library.Peripherals
{
    using Meadow.Hardware;
    using System;
    using System.Diagnostics;
    using System.Linq;

    internal class Mcp9808Comms
    {
        private readonly byte address;

        private readonly II2cBus i2c;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mcp9808Comms"/> class.
        /// </summary>
        /// <param name="i2c">The i2c.</param>
        /// <param name="busAddress">The bus address.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">busAddress - Invalid MCP9808 I2C bus address: 0x{busAddress:X2}</exception>
        /// <exception cref="System.ArgumentNullException">i2c</exception>
        internal Mcp9808Comms(II2cBus i2c, Mcp9808.I2cAddress busAddress)
        {
            if (!Enum<Mcp9808.I2cAddress>.GetValues().Contains(busAddress))
            {
                throw new ArgumentOutOfRangeException(nameof(busAddress), $"Invalid MCP9808 I2C bus address: 0x{busAddress:X2}");
            }

            this.i2c = i2c ?? throw new ArgumentNullException(nameof(i2c));
            this.address = (byte)busAddress;
        }

        /// <summary>
        /// Reads a <see cref="byte"/> from the specified register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <returns><see cref="byte"/>.</returns>
        public byte ReadRegisterByte(Register register)
        {
            byte[] result = this.ReadRegisterBytes(register, 1);

            Debug.Assert(result.Length == 1);

            return result[0];
        }

        /// <summary>
        /// Reads an array of <see cref="byte"/> values from the specified register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <param name="readCount">The amount of bytes to be read.</param>
        /// <returns><see cref="byte[]"/>.</returns>
        public byte[] ReadRegisterBytes(Register register, int readCount)
        {
            return this.i2c.WriteReadData(this.address, readCount, (byte)register);
        }

        /// <summary>
        /// Reads a <see cref="ushort"/> (2 bytes) from the specified register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <returns><see cref="ushort"/>.</returns>
        public ushort ReadRegisterUInt16(Register register)
        {
            byte[] result = this.ReadRegisterBytes(register, 2);

            Debug.Assert(result.Length == 2);

            return BitConverter.ToUInt16(result.Reverse().ToArray());
        }

        /// <summary>
        /// Write a <see cref="ushort"/> (2 bytes) value to the specified register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <param name="value">The value.</param>
        public void WriteRegister(Register register, ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            this.WriteRegister(register, bytes[0], bytes[1]);
        }

        /// <summary>
        /// Writes a <see cref="byte"/> value to the specified register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <param name="value">The value.</param>
        public void WriteRegister(Register register, byte value)
        {
            this.i2c.WriteData(this.address, (byte)register, value);
        }

        /// <summary>
        /// Writes two <see cref="byte"/> values to the specified register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <param name="value1">The value1.</param>
        /// <param name="value2">The value2.</param>
        public void WriteRegister(Register register, byte value1, byte value2)
        {
            this.i2c.WriteData(this.address, (byte)register, value1, value2);
        }

        internal enum Configurations : ushort
        {
            AlertOutputMode = 0x0001,

            AlertOutputPolarity = 0x0002,

            AlertOutputSelect = 0x0004,

            AlertOutputControl = 0x0008,

            AlertOutputStatus = 0x0010,

            InterruptClear = 0x0020,

            AlarmWindowLock = 0x0040,

            CriticalTripLock = 0x0080,

            Shutdown = 0x0100,

            WakeUp = unchecked((ushort)~(ushort)Configurations.Shutdown),
        }

        /// <summary>
        /// Registers used to control the MCP9808.
        /// </summary>
        internal enum Register : byte
        {
            Configuration = 0x01,

            AlertUpperBoundary = 0x02,

            AlertLowerBoundary = 0x03,

            CriticalTemperature = 0x04,

            AmbientTemperature = 0x05,

            ManufacturerId = 0x06,

            DeviceId = 0x07,

            Resolution = 0x08,
        }
    }
}
