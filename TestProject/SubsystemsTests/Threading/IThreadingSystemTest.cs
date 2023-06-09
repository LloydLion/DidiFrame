using DidiFrame.Threading;
using System.Threading;

namespace TestProject.SubsystemsTests.Threading
{
	public abstract class IThreadingSystemTest<TImplementation> where TImplementation : IThreadingSystem
	{
		private const int CommonTaskTimeout = 350;


		protected abstract TImplementation CreateNewSystem();


		[Test]
		public void CreateNewThread()
		{
			var system = CreateNewSystem();

			Assert.DoesNotThrow(() => system.CreateNewThread());
		}

		[Test]
		public void CreateNewQueue()
		{
			var system = CreateNewSystem();

			var thread = system.CreateNewThread();

			Assert.DoesNotThrow(() => thread.CreateNewExecutionQueue("default"));
		}

		[Test]
		public void BeginAndStopThread()
		{
			var system = CreateNewSystem();

			var thread = system.CreateNewThread();

			var queue = thread.CreateNewExecutionQueue("default");

			Assert.DoesNotThrow(() => thread.Begin(queue));	

			queue.Dispatch(thread.Stop);
		}

		[Test]
		public void DispatchTask()
		{
			var system = CreateNewSystem();

			var thread = system.CreateNewThread();

			var queue = thread.CreateNewExecutionQueue("default");

			Assert.DoesNotThrow(() => queue.Dispatch(() => { }));
		}

		[Test]
		public void ExecuteTask()
		{
			var system = CreateNewSystem();

			var thread = system.CreateNewThread();

			var queue = thread.CreateNewExecutionQueue("default");

			thread.Begin(queue);

			var resetEvent = new AutoResetEvent(false);

			queue.Dispatch(task);

			try
			{
				if (resetEvent.WaitOne(CommonTaskTimeout) == false)
					Assert.Fail($"Task was not executed in {CommonTaskTimeout}ms timeout");
			}
			finally
			{
				queue.Dispatch(thread.Stop);
			}



			void task()
			{
				resetEvent.Set();
			}
		}

		[Test]
		public void ChangeQueue()
		{
			var system = CreateNewSystem();

			var thread = system.CreateNewThread();

			var queue1 = thread.CreateNewExecutionQueue("queue1");
			var queue2 = thread.CreateNewExecutionQueue("queue2");

			var task1call = new AutoResetEvent(false);
			var task2call = false;
			var task3call = new AutoResetEvent(false);


			queue1.Dispatch(task1);
			queue1.Dispatch(task2);
			queue2.Dispatch(task3);

			thread.Begin(queue1);


			try
			{
				if (task1call.WaitOne(CommonTaskTimeout) == false)
					Assert.Fail($"Task #1 was not executed in {CommonTaskTimeout}ms timeout");

				Thread.Sleep(CommonTaskTimeout);

				if (task3call.WaitOne(CommonTaskTimeout) == false)
					Assert.Fail($"Task #3 was not executed in {CommonTaskTimeout}ms timeout");

				if (task2call)
					Assert.Fail($"Task #2 was executed, when shouldn't have");
			}
			finally
			{
				if (queue1.IsDisposed == false)
					queue1.Dispatch(thread.Stop);

				if (queue2.IsDisposed == false)
					queue2.Dispatch(thread.Stop);
			}



			void task1()
			{
				task1call.Set();
				thread.SetExecutionQueue(queue2);
			}

			void task2()
			{
				task2call = true;
			}

			void task3()
			{
				task3call.Set();
			}
		}
	}
}
