namespace MeadowWorkshop
{
    using System;
    using System.Threading;
    using Meadow;
    using Meadow.Devices;
    using Meadow.Hardware;

    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private IDigitalOutputPort redLed;
        private IDigitalOutputPort blueLed;
        private IDigitalOutputPort greenLed;

        public MeadowApp()
        {
            this.ConfigurePorts();
            this.BlinkLeds();
        }

        public void ConfigurePorts()
        {
            Console.WriteLine("Creating Outputs...");
            this.redLed = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedRed);
            this.blueLed = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedBlue);
            this.greenLed = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedGreen);
        }

        public void BlinkLeds()
        {
            bool state = false;

            while (true)
            {
                const int wait = 200;

                state = !state;

                Console.WriteLine($"State: {state}");

                this.redLed.State = state;
                Thread.Sleep(wait);
                this.blueLed.State = state;
                Thread.Sleep(wait);
                this.greenLed.State = state;
                Thread.Sleep(wait);
            }
        }
    }
}
