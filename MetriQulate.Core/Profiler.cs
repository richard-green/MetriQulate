using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using EasyNetQ;

namespace MetriQulate.Core
{
	public sealed class Profiler
	{
		#region Static Members

		private static object _mutex = new object();
		private static Profiler _instance;
		private static IBus _bus;

		public static Profiler Instance { get { return _instance; } }

		public static void EnableProfiling(IBus bus)
		{
			if (_instance == null)
			{
				lock (_mutex)
				{
					if (_instance == null)
					{
						_instance = new Profiler();
						_bus = bus;
					}
				}
			}
		}

		public static void EnableProfiling(string rabbitConnectionString)
		{
			EnableProfiling(RabbitHutch.CreateBus(rabbitConnectionString));
		}

		public static void DisableProfiling()
		{
			_instance = null;
		}

		#endregion Static Members

		#region Instance Members

		private Stopwatch stopwatch;
		private Timing currentTimer;

		internal Timing Timer(string name)
		{
			if (currentTimer == null)
			{
				stopwatch = Stopwatch.StartNew();
			}

			currentTimer = new Timing(name, this, currentTimer, stopwatch.ElapsedMilliseconds);
			return currentTimer;
		}

		internal void StopTimer(Timing timing)
		{
			currentTimer = timing.Parent;

			if (currentTimer == null)
			{
				stopwatch.Stop();

				using (var channel = _bus.OpenPublishChannel())
				{
					channel.Publish(timing.GetResults());
				}
			}
		}

		internal long Elapsed
		{
			get
			{
				return (stopwatch == null) ? 0 : stopwatch.ElapsedMilliseconds;
			}
		}

		#endregion Instance Members
	}
}
