using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace ConsoleApp5
{
    class Program
    {
        static bool _running = false;

        static WattageResults ComputeWattageResults(List<double> readings)
        {
            WattageResults ret = null;
            ret = new WattageResults
            { 
                NumberOfSamples = readings.Count,
                MeanWatts = readings.Average(),
                ModeWholeWatt = readings.Select(n=>(int)n).GroupBy(n=>n).OrderByDescending(n=>n.Count()).First().Key,
                MedianWatts = readings.OrderByDescending(n=>n).ToArray()[readings.Count/2], // not worrying about average the middle two
                StdDev = readings.StdDev(),
                MinWattage = readings.Min(),
                MaxWattage = readings.Max(),
            };
            return ret; 
        }

        static void Main(string[] args)
        {
            int percentageOn = 0, percentageOff = 0, targetPercentage = 0;
            if(args.Length == 2)
            {
                percentageOn = targetPercentage = Math.Min(100, Math.Max(int.Parse(args[0]), 0));
                percentageOff = 100 - percentageOn;
                Console.WriteLine($"Attempting to hit {percentageOn}% CPU on all.");
            }
            else
            {
                Console.Write("Percentage to hit: ");
                percentageOn = targetPercentage = Math.Min(100, Math.Max(int.Parse(Console.ReadLine()), 0)); ;
                percentageOff = 100 - percentageOn;
            }


            Console.WriteLine($"Milliseconds to operate: {percentageOn}");
            Console.WriteLine($"Milliseconds to sleep: {percentageOff}");

            using (IntelPowerGadget api = new IntelPowerGadget())
            {
                api.Start();

                // give a bit more to the percentage on, and take it away from percentage off. 
                // there's some busywork that I thik contributes to idle time around the timing logic. 
                float overhead = percentageOn * float.Parse(ConfigurationManager.AppSettings["scaler"]);

                percentageOn = Math.Min(100, percentageOn + (int)overhead);
                percentageOff = Math.Max(0, percentageOff - (int)overhead);

                _running = true;

                Task computation = Task.Run(() =>
                {
                    Parallel.For(0, Environment.ProcessorCount, (int i) =>
                    {
                        Stopwatch sw = new Stopwatch();

                        while (_running)
                        {
                            // work for n milliseconds
                            int v = 0;
                            sw.Start();
                            while (sw.ElapsedMilliseconds <= percentageOn)
                            {
                                v++;
                            }
                            sw.Reset();

                            // sleep for n milliseconds
                            Thread.Sleep(percentageOff);
                        }
                    });
                });

                // give a moment for things to spin up. 
                Thread.Sleep(1000);

                List<double> readings = new List<double>(); 
                Task powerMonitor = Task.Run(() =>
                {
                    while(_running)
                    {
                        double wattage = api.ReadWattage(); 
                        if(wattage > 1)
                        {
                            readings.Add(wattage); 
                        }
                        Thread.Sleep(500); 
                    }
                }); 

                Console.WriteLine("Press ENTER to stop.");
                Console.ReadLine();

                _running = false;
                computation.Wait();

                Console.WriteLine($"Overview:\n{ComputeWattageResults(readings).ToHumanReadableString()}");
                Console.Write("\nSave readings (y/N): ");
                if(Console.ReadLine().ToLower() == "y")
                {
                    File.WriteAllLines($"results_{targetPercentage}.csv", readings.Select(n => n.ToString())); 
                }
                Console.WriteLine("Press ENTER to exit.");
                Console.ReadLine(); 
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            _running = false;
        }
    }
}
