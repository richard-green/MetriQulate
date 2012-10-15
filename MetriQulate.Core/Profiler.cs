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

		#region Private Constructor

		private Profiler()
		{
		}

		#endregion Private Constructor

		#region Instance Members

		private ConcurrentDictionary<int, Timing> threadTimings = new ConcurrentDictionary<int, Timing>();

		internal Timing Timer(string typeName, string methodName, string timerName = null)
		{
			var threadId = Thread.CurrentThread.ManagedThreadId;
			Timing currentTiming = null;
			threadTimings.TryGetValue(threadId, out currentTiming);
			currentTiming = new Timing(typeName, methodName, timerName, this, currentTiming, threadId);
			threadTimings[threadId] = currentTiming;
			return currentTiming;
		}

		internal void StopTimer(Timing timing)
		{
			var threadId = Thread.CurrentThread.ManagedThreadId;

			if (timing.Parent == null)
			{
				using (var channel = _bus.OpenPublishChannel())
				{
					channel.Publish(timing.GetResults());
				}

				threadTimings.TryRemove(threadId, out timing);
			}
			else
			{
				threadTimings[threadId] = timing.Parent;
			}
		}

		#endregion Instance Members
	}
}
