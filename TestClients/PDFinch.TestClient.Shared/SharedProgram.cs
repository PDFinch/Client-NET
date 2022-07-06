using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PDFinch.TestClient.Shared
{
    public static class SharedProgram
    {
        public static async Task MainAsync()
        {
            Directory.CreateDirectory("Temp");
            
            // For date formatting.
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            
            do
            {
                await RunTestsAsync();

                Console.Write("Done! Press Enter to try again or D to open the output directory... ");

                ConsoleKeyInfo key;

                do
                {
                    key = Console.ReadKey(intercept: true);

                    // Open file explorer on D
                    // TODO: assumes Windows
                    if (key.Key == ConsoleKey.D)
                    {
                        Process.Start("explorer.exe", "Temp");
                    }
                }
                while (key.Key != ConsoleKey.Enter);

            } while (true);
        }

        private static async Task RunTestsAsync()
        {
            // #1: Your typical scenario: inject stuff from config into DI.
            await RunTestAsync(async() => await DependencyInjectionSample.RunAsync());
            
            // #2: Same as #1, but with a single client. Tests some other scenarios.
            await RunTestAsync(async() => await DependencyInjectionSample.RunSingleClientAsync());

            // #3: Manually register, create and maintain clients.
            await RunTestAsync(async() => await DirectUsageSample.RunAsync());
        }

        private static async Task RunTestAsync(Func<Task> action)
        {
            Stopwatch sw = new();
            sw.Reset();
            sw.Start();
            await action();
            Console.WriteLine($"Took {sw.ElapsedMilliseconds} ms");
        }
    }
}
