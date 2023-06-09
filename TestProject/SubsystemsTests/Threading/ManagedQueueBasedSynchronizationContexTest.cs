using DidiFrame.Threading;
using System;
using System.Collections.Generic;
using System.Threading;

namespace TestProject.SubsystemsTests.Threading
{
	public class ManagedQueueBasedSynchronizationContextTest
	{
		[Test]
		public void CreateContext()
		{
			var queue = new PseudoQueue();

			var context = new ManagedQueueBasedSynchronizationContext(queue);

			Assert.That(context.Queue, Is.EqualTo(queue));
		}

		[Test]
		public void PostCallback()
		{
			var queue = new PseudoQueue();

			var context = new ManagedQueueBasedSynchronizationContext(queue);

			context.Post(postCallback, null);

			Assert.That(queue.OrderedTaskList, Has.Count.EqualTo(1));



			void postCallback(object? nullState)
			{
				// Method intentionally left empty.
			}
		}

		[Test]
		public void CopyContext()
		{
			var queue = new PseudoQueue();

			var context = new ManagedQueueBasedSynchronizationContext(queue);

			var copy = context.CreateCopy();

			Assert.That(copy, Is.InstanceOf<ManagedQueueBasedSynchronizationContext>());
			Assert.That(((ManagedQueueBasedSynchronizationContext)copy).Queue, Is.EqualTo(queue));
		}


		[Test]
		public void SendCallbackInternal()
		{
			var queue = new PseudoQueue();

			queue.OwnerThread.IsInside = true;

			var context = new ManagedQueueBasedSynchronizationContext(queue);

			bool isCallbackExecuted = false;

			context.Send(postCallback, null);

			Assert.That(isCallbackExecuted, Is.True, "Callback was not executed by context");



			void postCallback(object? nullState)
			{
				isCallbackExecuted = true;
			}
		}


		[Test]
		public void SendCallbackExternal()
		{
			var queue = new PseudoQueue(shouldExecuteDispatched: true);

			queue.OwnerThread.IsInside = false;

			var context = new ManagedQueueBasedSynchronizationContext(queue);

			bool isCallbackExecuted = false;

			context.Send(postCallback, null);

			Assert.That(isCallbackExecuted, Is.True, "Callback was not executed by context");
			Assert.That(queue.OrderedTaskList, Has.Count.EqualTo(1));



			void postCallback(object? nullState)
			{
				isCallbackExecuted = true;
			}
		}


		private class PseudoQueue : IManagedThreadExecutionQueue
		{
			private readonly PseudoThread owner;
			private readonly bool shouldExecuteDispatched;


			public PseudoQueue(PseudoThread owner, bool shouldExecuteDispatched = false)
			{
				this.owner = owner;
				this.shouldExecuteDispatched = shouldExecuteDispatched;
			}

			public PseudoQueue(bool shouldExecuteDispatched = false) : this(new PseudoThread(), shouldExecuteDispatched) { }


			public bool IsDisposed => false;

			public IManagedThread Thread => owner;

			public List<ManagedThreadTask> OrderedTaskList { get; } = new();

			public PseudoThread OwnerThread => owner;


			public void Dispatch(ManagedThreadTask task)
			{
				if (shouldExecuteDispatched) task();
				OrderedTaskList.Add(task);
			}
		}

		private class PseudoThread : IManagedThread
		{
			public bool IsInside { get; set; }


			public void Begin(IManagedThreadExecutionQueue queue)
			{
				throw new NotImplementedException();
			}

			public IManagedThreadExecutionQueue CreateNewExecutionQueue(string queueName)
			{
				throw new NotImplementedException();
			}

			public IManagedThreadExecutionQueue GetActiveQueue()
			{
				throw new NotImplementedException();
			}

			public void SetExecutionQueue(IManagedThreadExecutionQueue queue)
			{
				throw new NotImplementedException();
			}

			public void Stop()
			{
				throw new NotImplementedException();
			}
		}
	}
}
