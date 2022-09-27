using DidiFrame.Testing.Utils;
using DidiFrame.UserCommands.Models;
using DidiFrame.UserCommands.Pipeline;
using System;

namespace TestProject.SubsystemsTests.UserCommands.Pipeline
{
	public abstract class IUserCommandPipelineDispatcherTests<TOut> where TOut : notnull
	{
		protected abstract UserCommandPipelineDispatcherTrigger CreateDispatcherTrigger();


		[Test]
		public void CallCallback()
		{
			using var trigger = CreateDispatcherTrigger();

			var dispatcher = trigger.CreateDispatcher();

			var key = new InvokationKey();

			dispatcher.SetSyncCallback(callback);

			trigger.TriggerDispatcher();

			Assert.That(key.SetCount, Is.EqualTo(1));



			void callback(IUserCommandPipelineDispatcher<TOut> invoker, TOut dispatcherOutput, UserCommandSendData sendData, object stateObject)
			{
				Assert.That(invoker, Is.EqualTo(dispatcher));

				key.Set();

				dispatcher.FinalizePipeline(stateObject);
			}
		}

		[Test]
		public void Respond()
		{
			int multiEnterPreventCounter = 0;

			var trigger = CreateDispatcherTrigger();

			var dispatcher = trigger.CreateDispatcher();

			var key = new InvokationKey();

			dispatcher.SetSyncCallback(callback);

			trigger.TriggerDispatcher();

			Assert.That(key.SetCount, Is.EqualTo(1));



			void callback(IUserCommandPipelineDispatcher<TOut> invoker, TOut dispatcherOutput, UserCommandSendData sendData, object stateObject)
			{
				if (multiEnterPreventCounter != 0) return;
				multiEnterPreventCounter++;

				Assert.That(invoker, Is.EqualTo(dispatcher));

				key.Set();

				respond();

				dispatcher.FinalizePipeline(stateObject);

				Assert.Throws<AggregateException>(respond);

				multiEnterPreventCounter--;



				void respond() { dispatcher.RespondAsync(stateObject, UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new("Some message"))).Wait(); }
			}
		}


		protected abstract class UserCommandPipelineDispatcherTrigger : IDisposable
		{
			public abstract IUserCommandPipelineDispatcher<TOut> CreateDispatcher();

			public abstract void Dispose();

			public abstract void TriggerDispatcher();
		}
	}
}
