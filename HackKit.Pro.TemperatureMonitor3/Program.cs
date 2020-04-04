namespace HackKit.Pro.TemperatureMonitor3
{
    using Meadow;
    using System;
    using System.Threading;

    public static class Program
    {
        private static IApp app;

        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--exitOnDebug")
            {
                return;
            }

            // instantiate and run new meadow app
            try
            {
                app = new MeadowApp();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
