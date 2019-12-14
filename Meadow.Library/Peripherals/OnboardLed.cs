namespace Meadow.Library.Peripherals
{
    using Meadow.Devices;
    using Meadow.Hardware;
    using System;

    public class OnboardLed : IDisposable
    {
        private readonly IDigitalOutputPort blueLed;

        private readonly IDigitalOutputPort greenLed;

        private readonly IDigitalOutputPort redLed;

        /// <summary>
        /// Gets the current LED color.
        /// </summary>
        public RgbColor Current =>
            (this.blueLed.State ? RgbColor.Blue : RgbColor.None)
            | (this.greenLed.State ? RgbColor.Green : RgbColor.None)
            | (this.redLed.State ? RgbColor.Red : RgbColor.None);

        /// <summary>
        /// Initializes a new instance of the <see cref="OnboardLed"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        public OnboardLed(F7Micro device)
        {
            this.redLed = device.CreateDigitalOutputPort(device.Pins.OnboardLedRed, initialState: false);
            this.blueLed = device.CreateDigitalOutputPort(device.Pins.OnboardLedBlue, initialState: false);
            this.greenLed = device.CreateDigitalOutputPort(device.Pins.OnboardLedGreen, initialState: false);
        }

        /// <summary>
        /// Sets the color.
        /// </summary>
        /// <param name="color">The color.</param>
        public void SetColor(RgbColor color)
        {
            this.blueLed.State = (color & RgbColor.Blue) != 0;
            this.greenLed.State = (color & RgbColor.Green) != 0;
            this.redLed.State = (color & RgbColor.Red) != 0;
        }

        /// <summary>
        /// Turns the LED off.
        /// </summary>
        public void Off()
        {
            this.SetColor(RgbColor.None);
        }

        #region IDisposable Support

        private bool disposedValue = false;

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.Off();
                    this.redLed.Dispose();
                    this.blueLed.Dispose();
                    this.greenLed.Dispose();
                }

                this.disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}
