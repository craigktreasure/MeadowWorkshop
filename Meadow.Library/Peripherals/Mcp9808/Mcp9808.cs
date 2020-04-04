namespace Meadow.Library.Peripherals
{
    using Meadow.Devices;
    using Meadow.Foundation.Sensors;
    using Meadow.Hardware;
    using Meadow.Peripherals.Sensors.Atmospheric;
    using Meadow.Peripherals.Sensors.Temperature;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    public class Mcp9808 :
        FilterableObservableBase<AtmosphericConditionChangeResult, AtmosphericConditions>,
        ITemperatureSensor
    {
        private const I2cAddress defaultI2cAddress = I2cAddress.Adddress0x18;

        private readonly Mcp9808Comms mcp9808Comms;

        // internal thread lock
        private readonly object _lock = new object();

        private CancellationTokenSource SamplingTokenSource;

        /// <summary>
        /// The AtmosphericConditions from the last reading.
        /// </summary>
        public AtmosphericConditions Conditions { get; protected set; } = new AtmosphericConditions();

        /// <summary>
        /// Gets a value indicating whether the device is currently being sampled.
        /// Call StartSampling() to start the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; private set; } = false;

        /// <summary>
        /// The temperature, in degrees celsius (°C), from the last reading.
        /// </summary>
        public float Temperature => this.GetTemperature().Temperature;

        public event EventHandler<AtmosphericConditionChangeResult> Updated;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mcp9808"/> class.
        /// </summary>
        /// <param name="i2c">The i2c.</param>
        /// <param name="busAddress">The bus address.</param>
        public Mcp9808(II2cBus i2c, I2cAddress busAddress = defaultI2cAddress)
        {
            this.mcp9808Comms = new Mcp9808Comms(i2c, busAddress);
        }

        /// <summary>
        /// Create a new <see cref="Mcp9808"/> using default I2C settings for the <see cref="F7Micro"/>.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="i2CAddress">The I2C address.</param>
        /// <returns><see cref="Mcp9808"/>.</returns>
        public static Mcp9808 FromF7Micro(F7Micro device, I2cAddress i2CAddress = defaultI2cAddress)
        {
            II2cBus i2c = device.CreateI2cBus();
            return new Mcp9808(i2c, i2CAddress);
        }

        /// <summary>
        /// Gets the device identifier.
        /// </summary>
        /// <returns><see cref="string"/>.</returns>
        public string GetDeviceId()
        {
            byte[] results = this.mcp9808Comms.ReadRegisterBytes(Mcp9808Comms.Register.DeviceId, 2);

            Debug.Assert(results.Length == 2);

            return $"0x{results[0]:X2}";
        }

        /// <summary>
        /// Gets the device revision.
        /// </summary>
        /// <returns><see cref="string"/>.</returns>
        public string GetDeviceRevision()
        {
            byte[] results = this.mcp9808Comms.ReadRegisterBytes(Mcp9808Comms.Register.DeviceId, 2);

            Debug.Assert(results.Length == 2);

            return $"0x{results[1]:X2}";
        }

        /// <summary>
        /// Gets the manufacturer identifier.
        /// </summary>
        /// <returns><see cref="string"/>.</returns>
        public string GetManufacturerId()
        {
            ushort result = this.mcp9808Comms.ReadRegisterUInt16(Mcp9808Comms.Register.ManufacturerId);
            return $"0x{result:X4}";
        }

        /// <summary>
        /// Gets the resolution.
        /// </summary>
        /// <returns><see cref="Resolution"/>.</returns>
        public Resolution GetResolution()
        {
            return (Resolution)this.mcp9808Comms.ReadRegisterByte(Mcp9808Comms.Register.Resolution);
        }

        /// <summary>
        /// Gets the temperature.
        /// </summary>
        /// <returns><see cref="AtmosphericConditions"/>.</returns>
        public AtmosphericConditions GetTemperature()
        {
            ushort rawResult = this.mcp9808Comms.ReadRegisterUInt16(Mcp9808Comms.Register.AmbientTemperature);

            // Clear flag bits.
            float temp = rawResult & 0x0FFF;
            temp /= 16.0f;

            if ((rawResult & 0x1000) == 0x1000)
            {
                temp -= 256;
            }

            this.Conditions = new AtmosphericConditions(temp, 0, 0);

            return AtmosphericConditions.From(this.Conditions);
        }

        /// <summary>
        /// Sets the resolution.
        /// </summary>
        /// <param name="resolution">The resolution.</param>
        public void SetResolution(Resolution resolution)
        {
            this.mcp9808Comms.WriteRegister(Mcp9808Comms.Register.Resolution, (byte)((byte)resolution & 0x03));
        }

        /// <summary>
        /// Shutdown the MCP9808.
        /// </summary>
        public void Shutdown()
        {
            byte[] configurationBytes = this.mcp9808Comms.ReadRegisterBytes(Mcp9808Comms.Register.Configuration, 2);

            ushort configuration = BitConverter.ToUInt16(configurationBytes);

            ushort shutdown = (ushort)(configuration | (ushort)Mcp9808Comms.Configurations.Shutdown);

            this.mcp9808Comms.WriteRegister(Mcp9808Comms.Register.Configuration, shutdown);
        }

        public void StartUpdating(int standbyDuration = 1000)
        {
            lock (this._lock)
            {
                if (this.IsSampling)
                {
                    return;
                }

                // state muh-cheen
                this.IsSampling = true;

                this.SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = this.SamplingTokenSource.Token;

                AtmosphericConditions oldConditions;
                AtmosphericConditionChangeResult result;
                Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            // do task clean up here
                            this._observers.ForEach(x => x.OnCompleted());
                            break;
                        }

                        // capture history
                        oldConditions = this.Conditions;

                        // read
                        this.GetTemperature();

                        // build a new result with the old and new conditions
                        result = new AtmosphericConditionChangeResult(oldConditions, this.Conditions);

                        // let everyone know
                        this.RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(standbyDuration).ConfigureAwait(false);
                    }
                }, this.SamplingTokenSource.Token);
            }
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdating()
        {
            lock (this._lock)
            {
                if (!this.IsSampling)
                {
                    return;
                }

                this.SamplingTokenSource?.Cancel();

                // state muh-cheen
                this.IsSampling = false;
            }
        }

        /// <summary>
        /// Wakes up the MCP9808.
        /// </summary>
        public void WakeUp()
        {
            byte[] configurationBytes = this.mcp9808Comms.ReadRegisterBytes(Mcp9808Comms.Register.Configuration, 2);

            ushort configuration = BitConverter.ToUInt16(configurationBytes);

            ushort wakup = (ushort)(configuration & (ushort)Mcp9808Comms.Configurations.WakeUp);

            this.mcp9808Comms.WriteRegister(Mcp9808Comms.Register.Configuration, wakup);
        }

        private void RaiseChangedAndNotify(AtmosphericConditionChangeResult changeResult)
        {
            this.Updated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        public enum I2cAddress : byte
        {
            /// <summary>
            /// The default address.
            /// </summary>
            Adddress0x18 = 0x18, // Default

            /// <summary>
            /// The adddress 0x19, which is enabled when A0 is set to 1.
            /// </summary>
            Adddress0x19 = Adddress0x18 + AddressSelectPin.A0,

            /// <summary>
            /// The adddress 0x1A, which is enabled when A1 is set to 1.
            /// </summary>
            Adddress0x1A = Adddress0x18 + AddressSelectPin.A1,

            /// <summary>
            /// The adddress 0x1B, which is enabled when A0 and A1 are set to 1.
            /// </summary>
            Adddress0x1B = Adddress0x18 + AddressSelectPin.A0 + AddressSelectPin.A1,

            /// <summary>
            /// The adddress 0x1C, which is enabled when A2 is set to 1.
            /// </summary>
            Adddress0x1C = Adddress0x18 + AddressSelectPin.A2,

            /// <summary>
            /// The adddress 0x1D, which is enabled when A0 and A2 are set to 1.
            /// </summary>
            Adddress0x1D = Adddress0x18 + AddressSelectPin.A0 + AddressSelectPin.A2,

            /// <summary>
            /// The adddress 0x1F, which is enabled when A0, A1, and A2 are set to 1.
            /// </summary>
            Adddress0x1F = Adddress0x18 + AddressSelectPin.A0 + AddressSelectPin.A1 + AddressSelectPin.A2,
        }

        public enum Resolution : byte
        {
            /// <summary>
            /// The low resolution mode: 0.5°C resolution at 30 ms sample rate.
            /// </summary>
            Low = 0,

            /// <summary>
            /// Medium resolution mode: 0.25°C resolution at 65 ms sample rate.
            /// </summary>
            Medium = 1,

            /// <summary>
            /// High resolution mode: 0.125°C resolution at 130 ms sample rate.
            /// </summary>
            High = 2,

            /// <summary>
            /// Best resolution mode (Power-up Default): 0.0625°C resolution at 250 ms sample rate.
            /// </summary>
            Best = 3, // Power-up Default
        }

        private enum AddressSelectPin : byte
        {
            A0 = 0x01,

            A1 = 0x02,

            A2 = 0x04,
        }
    }
}
