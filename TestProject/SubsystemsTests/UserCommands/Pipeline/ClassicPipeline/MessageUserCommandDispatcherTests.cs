using DidiFrame.Testing.Localization;
using DidiFrame.Testing.Logging;
using DidiFrame.UserCommands.Pipeline;
using DidiFrame.UserCommands.Pipeline.ClassicPipeline;
using Microsoft.Extensions.Options;

namespace TestProject.SubsystemsTests.UserCommands.Pipeline.ClassicPipeline
{
	internal class MessageUserCommandDispatcherTests : IUserCommandPipelineDispatcherTests<IMessage>
	{
		protected override UserCommandPipelineDispatcherTrigger CreateDispatcherTrigger()
		{
			return new Trigger();
		}


		private class Trigger : UserCommandPipelineDispatcherTrigger
		{
			private readonly Client client;
			private readonly TextChannelBase textChannel;
			private readonly Member member;
			

			public Trigger()
			{
				client = new Client();
				var server = client.CreateServer();
				textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));
				member = server.AddMember("Somebody", false, Permissions.All);
			}


			public override IUserCommandPipelineDispatcher<IMessage> CreateDispatcher()
			{
				return new MessageUserCommandDispatcher(client, new TestLocalizer<MessageUserCommandDispatcher>(), new DebugConsoleLogger<MessageUserCommandDispatcher>(),
					Options.Create(new MessageUserCommandDispatcher.Options() { DisableDeleteDelayForDebug = true }));
			}

			public override void Dispose()
			{
				client.Dispose();
			}

			public override void TriggerDispatcher()
			{
				textChannel.AddMessage(member, new("CommandContent"));
			}
		}
	}
}
