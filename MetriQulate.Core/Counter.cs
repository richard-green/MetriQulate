using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyNetQ;
using System.Collections.Concurrent;
using System.Threading;

namespace MetriQulate.Core
{
	public class Counter
	{
		#region Static Members

		private static object _mutex = new object();
		private static Counter _instance;
		private static IBus _bus;

		public static Counter Instance { get { return _instance; } }

		/// <summary>
		/// Turn on code profiling
		/// </summary>
		/// <param name="bus"></param>
		/// <param name="counterPolling"></param>
		public static void EnableReporting(IBus bus, double notifyInterval = 5000)
		{
			if (_instance == null)
			{
				lock (_mutex)
				{
					if (_instance == null)
					{
						_instance = new Counter(notifyInterval);
						_bus = bus;
					}
				}
			}
		}

		public static void EnableReporting(string rabbitConnectionString, double notifyInterval = 5000)
		{
			EnableReporting(RabbitHutch.CreateBus(rabbitConnectionString), notifyInterval);
		}

		public static void DisableReporting()
		{
			_instance = null;
		}

		#endregion Static Members

		#region Private Constructor

		private Counter(double notifyInterval)
		{
			this.notifyInterval = notifyInterval;
			this.notify = notifyInterval > 0;

			if (notify)
			{
				notifyTimer = new System.Timers.Timer();
				notifyTimer.Interval = notifyInterval;
				notifyTimer.AutoReset = false;
				notifyTimer.Enabled = true;
				notifyTimer.Elapsed += new System.Timers.ElapsedEventHandler(notifyTimer_Elapsed);
				notifyTimer.Start();
			}
		}

		#endregion Private Constructor

		#region Instance Members

		private bool notify;
		private double notifyInterval;
		private System.Timers.Timer notifyTimer;
		private Dictionary<string, CounterReport> currentCounters = new Dictionary<string, CounterReport>();

		internal void ReportFailure(string counterName)
		{
			Report(counterName, false);
		}

		internal void ReportSuccess(string counterName)
		{
			Report(counterName, true);
		}

		private void Report(string counterName, bool success)
		{
			CounterReport counterReport = null;

			if (currentCounters.TryGetValue(counterName, out counterReport) == false)
			{
				lock (currentCounters)
				{
					if (currentCounters.TryGetValue(counterName, out counterReport) == false)
					{
						counterReport = new CounterReport();
						currentCounters[counterName] = counterReport;
					}
				}
			}

			if (success)
			{
				System.Threading.Interlocked.Increment(ref counterReport.SuccessCount);
			}
			else
			{
				System.Threading.Interlocked.Increment(ref counterReport.FailureCount);
			}
		}

		internal void Disable()
		{
			notifyTimer.Enabled = false;
			notify = false;
		}

		private void notifyTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			try
			{
				if (notify)
				{
					CounterNotification notification = new CounterNotification();

					lock (currentCounters)
					{
						foreach (var kvp in currentCounters)
						{
							var report = new CounterReport()
							{
								Name = kvp.Key,
								SuccessCount = kvp.Value.SuccessCount,
								FailureCount = kvp.Value.FailureCount
							};
							if (report.SuccessCount > 0 || report.FailureCount > 0)
							{
								Interlocked.Add(ref kvp.Value.SuccessCount, -report.SuccessCount);
								Interlocked.Add(ref kvp.Value.FailureCount, -report.FailureCount);
								notification.Counters.Add(report);
							}
						}
					}

					if (notification.Counters.Any())
					{
						using (var channel = _bus.OpenPublishChannel())
						{
							channel.Publish(notification);
						}
					}
					else
					{
						Console.WriteLine("Nothing happened");
					}
				}
			}
			finally
			{
				notifyTimer.Start();
			}
		}

		#endregion Instance Members
	}
}
