using DidiFrame.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

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

			using var threadDiposable = system.CreateNewThread().CreateTestDisposable(out var thread);

			var queue = thread.CreateNewExecutionQueue("default");

			Assert.DoesNotThrow(() => thread.Begin(queue));
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

			using var threadDiposable = system.CreateNewThread().CreateTestDisposable(out var thread);

			var queue = thread.CreateNewExecutionQueue("default");

			thread.Begin(queue);

			var resetEvent = new AutoResetEvent(false);

			queue.Dispatch(task);

			if (resetEvent.WaitOne(CommonTaskTimeout) == false)
				Assert.Fail($"Task was not executed in {CommonTaskTimeout}ms timeout");



			void task()
			{
				resetEvent.Set();
			}
		}

		[Test]
		public void ChangeQueue()
		{
			var system = CreateNewSystem();

			using var threadDiposable = system.CreateNewThread().CreateTestDisposable(out var thread);

			var queue1 = thread.CreateNewExecutionQueue("queue1");
			var queue2 = thread.CreateNewExecutionQueue("queue2");

			var task1call = new AutoResetEvent(false);
			var task2call = false;
			var task3call = new AutoResetEvent(false);


			queue1.Dispatch(task1);
			queue1.Dispatch(task2);
			queue2.Dispatch(task3);

			thread.Begin(queue1);


			if (task1call.WaitOne(CommonTaskTimeout) == false)
				Assert.Fail($"Task #1 was not executed in {CommonTaskTimeout}ms timeout");

			Thread.Sleep(CommonTaskTimeout);

			if (task3call.WaitOne(CommonTaskTimeout) == false)
				Assert.Fail($"Task #3 was not executed in {CommonTaskTimeout}ms timeout");

			if (task2call)
				Assert.Fail($"Task #2 was executed, when shouldn't have");



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

		[Test]
		public void DisposeQueue()
		{
			var system = CreateNewSystem();

			using var threadDiposable = system.CreateNewThread().CreateTestDisposable(out var thread);

			var queue1 = thread.CreateNewExecutionQueue("queue1");
			var queue2 = thread.CreateNewExecutionQueue("queue2");

			var resetEvent = new AutoResetEvent(false);

			queue1.Dispatch(() =>
			{
				thread.SetExecutionQueue(queue2);
				resetEvent.Set();
			});

			thread.Begin(queue1);


			if (resetEvent.WaitOne(CommonTaskTimeout) == false)
				Assert.Fail($"Task was not executed in {CommonTaskTimeout}ms timeout");

			Assert.That(queue1.IsDisposed, Is.True);
			Assert.That(queue2.IsDisposed, Is.False);
			Assert.Throws<ObjectDisposedException>(() => queue1.Dispatch(() => { }));
		}

		[Test]
		public void CheckSynchronizationContext()
		{
			var system = CreateNewSystem();

			using var threadDiposable = system.CreateNewThread().CreateTestDisposable(out var thread);

			var queue = thread.CreateNewExecutionQueue("default");

			var resetEvent = new AutoResetEvent(false);
			SynchronizationContext? internalContext = null;

			queue.Dispatch(() =>
			{
				internalContext = SynchronizationContext.Current;
				resetEvent.Set();
			});

			thread.Begin(queue);


			if (resetEvent.WaitOne(CommonTaskTimeout) == false)
				Assert.Fail($"Task was not executed in {CommonTaskTimeout}ms timeout");

			Assert.That(internalContext, Is.InstanceOf<ManagedQueueBasedSynchronizationContext>());
			Assert.That(((ManagedQueueBasedSynchronizationContext)internalContext!).Queue, Is.EqualTo(queue));
		}
	}
}
