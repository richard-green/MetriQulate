using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using EasyNetQ;
using System.Threading;
using System.Collections.Concurrent;

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

		private ConcurrentDictionary<int, Timing> threadTimings = new ConcurrentDictionary<int, Timing>();

		internal Timing Timer(string name)
		{
			var threadId = Thread.CurrentThread.ManagedThreadId;
			var currentTimer = threadTimings.ContainsKey(threadId) ? threadTimings[threadId] : null;
			currentTimer = new Timing(name, this, currentTimer, threadId);
			threadTimings[threadId] = currentTimer;
			return currentTimer;
		}

		internal void StopTimer(Timing timing)
		{
			var threadId = Thread.CurrentThread.ManagedThreadId;
			threadTimings[threadId] = timing.Parent;

			if (timing.Parent == null)
			{
				using (var channel = _bus.OpenPublishChannel())
				{
					channel.Publish(timing.GetResults());
				}
			}
		}

		#endregion Instance Members
	}
}
