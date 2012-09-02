using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MetriQulate.Core;
using System.Threading;
using EasyNetQ;
using System.Diagnostics;

namespace MetriQulate.Test
{
	class TestProgram
	{
		static void Main(string[] args)
		{
			var bus = RabbitHutch.CreateBus("host=localhost");
			bus.Subscribe<TimingResult>("Timing", TimingReceived);

			Stopwatch stopwatch = Stopwatch.StartNew();

			// Comment out this line of code to disable profiling - profiling is disabled by default
			Profiler.EnableProfiling(bus);

			for (int i = 0; i < 100; i++)
			{
				using (Profiler.Instance.Timer("Timer 1"))
				{
					for (int j = 0; j < 10; j++)
					{
						using (Profiler.Instance.Timer("Timer 2"))
						{
							Thread.Sleep(5);

							using (Profiler.Instance.Timer("Timer 3"))
							{
								Thread.Sleep(5);
							}
						}
					}
				}
			}

			Console.WriteLine("Complete in {0:n0}ms", stopwatch.ElapsedMilliseconds);
		}

		private static void TimingReceived(TimingResult timing)
		{
			Console.WriteLine("Received:");
			DumpTiming(timing);
		}

		private static void DumpTiming(TimingResult timing, int nestLevel = 0)
		{
			if (timing.SubTimings != null && timing.SubTimings.Count > 0)
			{
				Console.WriteLine("{0} {1} - {2:n0}ms ({3:n0}ms)", new String('>', nestLevel * 2), timing.Name, timing.Elapsed, timing.ChildrenElapsed);
				foreach (var subTiming in timing.SubTimings)
				{
					DumpTiming(subTiming, nestLevel + 1);
				}
			}
			else
			{
				Console.WriteLine("{0} {1} - {2:n0}ms", new String('>', nestLevel * 2), timing.Name, timing.Elapsed, timing.ChildrenElapsed);
			}
		}
	}
}
