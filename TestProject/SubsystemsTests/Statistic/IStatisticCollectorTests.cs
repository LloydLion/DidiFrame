using DidiFrame.Statistic;
using System;

namespace TestProject.SubsystemsTests.Statistic
{
	public abstract class IStatisticCollectorTests
	{
		public abstract IStatisticCollector CreateCollector();


		[Test]
		public void CollectData()
		{
			var client = new Client();
			var server = client.CreateServer();

			var collector = CreateCollector();
			var random = new Random(100);

			var entry = new StatisticEntry("test", 0);

			//-----------------

			for (int i = 0; i < 1000; i++)
			{
				var value = random.Next();
				collector.Collect((ref long data) => data = value, entry, server);
				var actualValue = collector.Get(entry, server);
				Assert.That(actualValue, Is.EqualTo(value));
			}
		}

		[Test]
		public void DefaultValue()
		{
			var client = new Client();
			var server = client.CreateServer();

			var collector = CreateCollector();

			var entry = new StatisticEntry("test", 25);

			//-----------------

			Assert.That(collector.Get(entry, server), Is.EqualTo(25));

			collector.Collect((ref long value) =>
			{
				Assert.That(value, Is.EqualTo(entry.DefaultValue));
			}, entry, server);
		}

		[Test]
		public void ValidInActionValue()
		{
			var client = new Client();
			var server = client.CreateServer();

			var collector = CreateCollector();

			var entry = new StatisticEntry("test", 0);

			//-----------------

			var realValue = entry.DefaultValue;

			for (int i = 0; i < 100; i++)
			{
				collector.Collect((ref long value) =>
				{
					Assert.That(value, Is.EqualTo(realValue));
					realValue++;
					value++;
				}, entry, server);
			}
		}
	}
}
