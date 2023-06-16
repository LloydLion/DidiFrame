using DidiFrame.Exceptions;
using DidiFrame.Utils.RoutedEvents;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestProject.SubsystemsTests.Utils.RoutedEvents
{
	public class RoutedEventTreeNodeTest
	{
		private readonly RoutedEvent<TestEventArgs> BubblingEvent = new RoutedEvent<TestEventArgs>(typeof(TestEventArgs), nameof(BubblingEvent), RoutedEvent.PropagationDirection.Bubbling);
		private readonly RoutedEvent<TestEventArgs> TunnelingEvent = new RoutedEvent<TestEventArgs>(typeof(TestEventArgs), nameof(TunnelingEvent), RoutedEvent.PropagationDirection.Tunneling);
		private readonly RoutedEvent<TestEventArgs> EventWithoutPropogation = new RoutedEvent<TestEventArgs>(typeof(TestEventArgs), nameof(EventWithoutPropogation), RoutedEvent.PropagationDirection.None);


		[Test]
		public async Task InvokeSimpleEvent()
		{
			var owner = new TestOwner((fucn) => fucn().AsTask());

			owner.AddListener(EventWithoutPropogation, eventHandler1);
			owner.AddListener(BubblingEvent, eventHandler2);
			owner.AddListener(TunnelingEvent, eventHandler3);

			var margs = new TestEventArgs();

			await owner.Node.Invoke(EventWithoutPropogation, margs);
			Assert.That(margs.Number, Is.EqualTo(1), "Event handler #1 was not called");

			await owner.Node.Invoke(BubblingEvent, margs);
			Assert.That(margs.Number, Is.EqualTo(2), "Event handler #2 was not called");

			await owner.Node.Invoke(TunnelingEvent, margs);
			Assert.That(margs.Number, Is.EqualTo(3), "Event handler #3 was not called");



			ValueTask eventHandler1(RoutedEventSender sender, TestEventArgs args)
			{
				args.Number = 1;
				return ValueTask.CompletedTask;
			}

			ValueTask eventHandler2(RoutedEventSender sender, TestEventArgs args)
			{
				args.Number = 2;
				return ValueTask.CompletedTask;
			}

			ValueTask eventHandler3(RoutedEventSender sender, TestEventArgs args)
			{
				args.Number = 3;
				return ValueTask.CompletedTask;
			}
		}

		[Test]
		public async Task UnSubscribeHandler()
		{
			var owner = new TestOwner((fucn) => fucn().AsTask());

			owner.AddListener(EventWithoutPropogation, eventHandler1);
			owner.AddListener(BubblingEvent, eventHandler2);
			owner.AddListener(TunnelingEvent, eventHandler3);

			var margs = new TestEventArgs();

			await owner.Node.Invoke(EventWithoutPropogation, margs);
			Assert.That(margs.Number, Is.EqualTo(1), "Event handler #1 was not called");

			margs.Number = 0;
			await owner.Node.Invoke(EventWithoutPropogation, margs);
			Assert.That(margs.Number, Is.EqualTo(0), "Event handler #1 was called after unSubscribtion");

			await owner.Node.Invoke(BubblingEvent, margs);
			Assert.That(margs.Number, Is.EqualTo(2), "Event handler #2 was not called");

			margs.Number = 0;
			await owner.Node.Invoke(TunnelingEvent, margs);
			Assert.That(margs.Number, Is.EqualTo(0), "Event handler #3 was called after unSubscribtion");



			ValueTask eventHandler1(RoutedEventSender sender, TestEventArgs args)
			{
				sender.UnSubscribeMyself();
				args.Number = 1;
				return ValueTask.CompletedTask;
			}

			ValueTask eventHandler2(RoutedEventSender sender, TestEventArgs args)
			{
				args.Number = 2;
				sender.Sender.RemoveListener(TunnelingEvent, eventHandler3);
				return ValueTask.CompletedTask;
			}

			ValueTask eventHandler3(RoutedEventSender sender, TestEventArgs args)
			{
				args.Number = 3;
				return ValueTask.CompletedTask;
			}
		}

		[Test]
		public async Task InvokeBubblingEvent()
		{
			var nodeL1 = new TestOwner((fucn) => fucn().AsTask());

			var nodeL2_1 = new TestOwner();
			var nodeL2_2 = new TestOwner();

			var nodeL3_1_1 = new TestOwner();
			var nodeL3_1_2 = new TestOwner();
			var nodeL3_2_1 = new TestOwner();
			var nodeL3_2_2 = new TestOwner();

			nodeL3_1_1.Node.AttachParent(nodeL2_1.Node);
			nodeL3_1_2.Node.AttachParent(nodeL2_1.Node);

			nodeL3_2_1.Node.AttachParent(nodeL2_2.Node);
			nodeL3_2_2.Node.AttachParent(nodeL2_2.Node);

			nodeL2_1.Node.AttachParent(nodeL1.Node);
			nodeL2_2.Node.AttachParent(nodeL1.Node);


			var margs = new TestEventArgs();

			nodeL1.AddListener(BubblingEvent, eventHandler1);
			nodeL2_1.AddListener(BubblingEvent, eventHandler2);

			nodeL3_1_1.AddListener(BubblingEvent, shouldNotInvoke);
			nodeL3_1_2.AddListener(BubblingEvent, shouldNotInvoke);
			nodeL3_2_1.AddListener(BubblingEvent, shouldNotInvoke);
			nodeL3_2_2.AddListener(BubblingEvent, shouldNotInvoke);
			nodeL2_2.AddListener(BubblingEvent, shouldNotInvoke);

			await nodeL2_1.Node.Invoke(BubblingEvent, margs);
			Assert.That(margs.Number, Is.EqualTo(1), "Handler #1 was not executed");
			Assert.That(margs.Number2, Is.EqualTo(1), "Handler #2 was not executed");



			ValueTask eventHandler1(RoutedEventSender sender, TestEventArgs args)
			{
				Assert.That(args.Number2, Is.EqualTo(1), "Handler execution order mismatch");
				args.Number = 1;
				return ValueTask.CompletedTask;
			}

			ValueTask eventHandler2(RoutedEventSender sender, TestEventArgs args)
			{
				args.Number2 = 1;
				return ValueTask.CompletedTask;
			}

			ValueTask shouldNotInvoke(RoutedEventSender sender, TestEventArgs args)
			{
				Assert.Fail("Executed handler that should not be executed");
				return ValueTask.CompletedTask;
			}
		}

		[Test]
		public async Task InvokeTunnelingEvent()
		{
			var nodeL1 = new TestOwner((fucn) => fucn().AsTask());

			var nodeL2_1 = new TestOwner();
			var nodeL2_2 = new TestOwner();

			var nodeL3_1_1 = new TestOwner();
			var nodeL3_1_2 = new TestOwner();
			var nodeL3_2_1 = new TestOwner();
			var nodeL3_2_2 = new TestOwner();

			nodeL3_1_1.Node.AttachParent(nodeL2_1.Node);
			nodeL3_1_2.Node.AttachParent(nodeL2_1.Node);

			nodeL3_2_1.Node.AttachParent(nodeL2_2.Node);
			nodeL3_2_2.Node.AttachParent(nodeL2_2.Node);

			nodeL2_1.Node.AttachParent(nodeL1.Node);
			nodeL2_2.Node.AttachParent(nodeL1.Node);


			var margs = new TestEventArgs();

			nodeL2_1.AddListener(TunnelingEvent, eventHandler1);
			nodeL3_1_1.AddListener(TunnelingEvent, eventHandler2);
			nodeL3_1_2.AddListener(TunnelingEvent, eventHandler3);

			nodeL1.AddListener(TunnelingEvent, shouldNotInvoke);
			nodeL3_2_1.AddListener(TunnelingEvent, shouldNotInvoke);
			nodeL3_2_2.AddListener(TunnelingEvent, shouldNotInvoke);
			nodeL2_2.AddListener(TunnelingEvent, shouldNotInvoke);

			await nodeL2_1.Node.Invoke(TunnelingEvent, margs);
			Assert.That(margs.Number, Is.EqualTo(1), "Handler #1 was not executed");
			Assert.That(margs.Number2, Is.EqualTo(1), "Handler #2 was not executed");
			Assert.That(margs.Number3, Is.EqualTo(1), "Handler #3 was not executed");



			ValueTask eventHandler1(RoutedEventSender sender, TestEventArgs args)
			{
				Assert.That(args.Number2, Is.EqualTo(0), "Handler execution order mismatch");
				Assert.That(args.Number3, Is.EqualTo(0), "Handler execution order mismatch");
				args.Number = 1;
				return ValueTask.CompletedTask;
			}

			ValueTask eventHandler2(RoutedEventSender sender, TestEventArgs args)
			{
				args.Number2 = 1;
				return ValueTask.CompletedTask;
			}

			ValueTask eventHandler3(RoutedEventSender sender, TestEventArgs args)
			{
				args.Number3 = 1;
				return ValueTask.CompletedTask;
			}

			ValueTask shouldNotInvoke(RoutedEventSender sender, TestEventArgs args)
			{
				Assert.Fail("Executed handler that should not be executed");
				return ValueTask.CompletedTask;
			}
		}

		[Test]
		public async Task InvokeEventWithoutPropagation()
		{
			var nodeL1 = new TestOwner((fucn) => fucn().AsTask());

			var nodeL2_1 = new TestOwner();
			var nodeL2_2 = new TestOwner();

			var nodeL3_1_1 = new TestOwner();
			var nodeL3_1_2 = new TestOwner();
			var nodeL3_2_1 = new TestOwner();
			var nodeL3_2_2 = new TestOwner();

			nodeL3_1_1.Node.AttachParent(nodeL2_1.Node);
			nodeL3_1_2.Node.AttachParent(nodeL2_1.Node);

			nodeL3_2_1.Node.AttachParent(nodeL2_2.Node);
			nodeL3_2_2.Node.AttachParent(nodeL2_2.Node);

			nodeL2_1.Node.AttachParent(nodeL1.Node);
			nodeL2_2.Node.AttachParent(nodeL1.Node);


			var margs = new TestEventArgs();

			nodeL2_1.AddListener(EventWithoutPropogation, eventHandler1);

			nodeL1.AddListener(EventWithoutPropogation, shouldNotInvoke);
			nodeL2_2.AddListener(EventWithoutPropogation, shouldNotInvoke);
			nodeL3_1_1.AddListener(EventWithoutPropogation, shouldNotInvoke);
			nodeL3_1_2.AddListener(EventWithoutPropogation, shouldNotInvoke);
			nodeL3_2_1.AddListener(EventWithoutPropogation, shouldNotInvoke);
			nodeL3_2_2.AddListener(EventWithoutPropogation, shouldNotInvoke);

			await nodeL2_1.Node.Invoke(EventWithoutPropogation, margs);
			Assert.That(margs.Number, Is.EqualTo(1), "Handler #1 was not executed");



			ValueTask eventHandler1(RoutedEventSender sender, TestEventArgs args)
			{
				args.Number = 1;
				return ValueTask.CompletedTask;
			}

			ValueTask shouldNotInvoke(RoutedEventSender sender, TestEventArgs args)
			{
				Assert.Fail("Executed handler that should not be executed");
				return ValueTask.CompletedTask;
			}
		}

		[Test]
		public void ThreadAccess()
		{
			var owner = new TestOwner((fucn) => fucn().AsTask());

			var otherThread = new TaskFactory(TaskScheduler.Default).StartNew(() =>
			{
				Assert.Throws<ThreadAccessException>(() => owner.Node.Invoke(EventWithoutPropogation, new()).Wait());
			});

			otherThread.Wait();
		}

		[Test]
		public async Task ExecutorDelegating()
		{
			int handler1ExecCount = 0;
			int handler2ExecCount = 0;


			var nodeL1 = new TestOwner();

			var nodeL2_1 = new TestOwner((func) => { handler1ExecCount++; return func().AsTask(); });
			var nodeL2_2 = new TestOwner((func) => { handler2ExecCount++; return func().AsTask(); });

			var nodeL3_1_1 = new TestOwner();
			var nodeL3_1_2 = new TestOwner();
			var nodeL3_2_1 = new TestOwner();
			var nodeL3_2_2 = new TestOwner();

			nodeL3_1_1.Node.AttachParent(nodeL2_1.Node);
			nodeL3_1_2.Node.AttachParent(nodeL2_1.Node);

			nodeL3_2_1.Node.AttachParent(nodeL2_2.Node);
			nodeL3_2_2.Node.AttachParent(nodeL2_2.Node);

			nodeL2_1.Node.AttachParent(nodeL1.Node);
			nodeL2_2.Node.AttachParent(nodeL1.Node);

			nodeL1.AddListener(BubblingEvent, stupHandler);
			nodeL2_1.AddListener(BubblingEvent, stupHandler);
			nodeL2_2.AddListener(BubblingEvent, stupHandler);
			nodeL3_1_1.AddListener(BubblingEvent, stupHandler);
			nodeL3_1_2.AddListener(BubblingEvent, stupHandler);
			nodeL3_2_1.AddListener(BubblingEvent, stupHandler);
			nodeL3_2_2.AddListener(BubblingEvent, stupHandler);


			var margs = new TestEventArgs();

			await nodeL3_1_1.Node.Invoke(BubblingEvent, margs);
			Assert.That(handler1ExecCount, Is.EqualTo(3));
			Assert.That(handler2ExecCount, Is.EqualTo(0));

			await nodeL3_2_2.Node.Invoke(BubblingEvent, margs);
			Assert.That(handler1ExecCount, Is.EqualTo(3));
			Assert.That(handler2ExecCount, Is.EqualTo(3));

			int handler3ExecCount = 0;
			await nodeL3_2_2.Node.Invoke(BubblingEvent, margs, (func) => { handler3ExecCount++; return func().AsTask(); });
			Assert.That(handler1ExecCount, Is.EqualTo(3));
			Assert.That(handler2ExecCount, Is.EqualTo(3));
			Assert.That(handler3ExecCount, Is.EqualTo(3));

			Assert.Throws<InvalidOperationException>(() => nodeL1.Node.Invoke(BubblingEvent, margs).Wait());



			static ValueTask stupHandler(RoutedEventSender sender, TestEventArgs args)
			{
				return ValueTask.CompletedTask;
			}
		}


		private class TestEventArgs : EventArgs
		{
			public int Number { get; set; }

			public int Number2 { get; set; }

			public int Number3 { get; set; }
		}

		private class TestOwner : IRoutedEventObject
		{
			private readonly RoutedEventTreeNode node;


			public TestOwner(RoutedEventTreeNode.HandlerExecutor? executor = null)
			{
				node = new RoutedEventTreeNode(this, executor);
			}


			public RoutedEventTreeNode Node => node;


			public void AddListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs
			{
				node.AddListener(routedEvent, handler);
			}

			public void RemoveListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs
			{
				node.RemoveListener(routedEvent, handler);
			}
		}
	}
}
