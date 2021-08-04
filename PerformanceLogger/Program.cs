using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Linq; 

namespace PerformanceLogger
{
    class Program
    {
        static void PrintSummary(List<float> timeInGc)
        {
            float totalTimeInGC = 100 * (timeInGc.Sum() / (timeInGc.Count * 100.0f));

            Console.WriteLine($"{totalTimeInGC}% of the time was spent GC'ing"); 
        }

        static void Main(string[] args)
        {
            using (StreamWriter sw = new StreamWriter("stats.csv", false))
            {
                for (; ; )
                {
                    Process[] procs = Process.GetProcessesByName("ClusterClient");
                    if (procs.Length == 1)
                    {
                        Console.WriteLine("Watching...");

                        List<float> memoryCounts = new List<float>(); 
                        PerformanceCounter clrMemoryCounter = new PerformanceCounter(".NET CLR Memory", "% Time in GC", "ClusterClient");
                        PerformanceCounter cpuCounter = new PerformanceCounter("Process", "% Processor Time", "ClusterClient");

                        sw.WriteLine("% Time in GC, % Processor Time");
                        while (!procs[0].HasExited)
                        {
                            float timeInGc = clrMemoryCounter.NextValue();
                            float processorTime = cpuCounter.NextValue();

                            memoryCounts.Add(timeInGc); 

                            sw.WriteLine($"{timeInGc}, {processorTime}");

                            Thread.Sleep(100); 
                        }

                        PrintSummary(memoryCounts); 

                        break;
                    }
                    else
                    {
                        // wait for the cluster client to start
                        Thread.Sleep(100);
                        Console.Write(".");
                    }
                }
            }
        }
    }
}
