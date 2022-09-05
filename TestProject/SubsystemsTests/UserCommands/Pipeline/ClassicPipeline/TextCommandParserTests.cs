using DidiFrame.UserCommands.Models;
using DidiFrame.UserCommands.Pipeline;
using DidiFrame.UserCommands.Pipeline.ClassicPipeline;
using DidiFrame.UserCommands.Repository;
using DidiFrame.Utils.ExtendableModels;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestProject.SubsystemsTests.UserCommands.Pipeline.ClassicPipeline
{
	internal class TextCommandParserTests
	{
		[Test]
		public async Task ParseSimpleCommand()
		{
			var client = new Client();
			var server = client.CreateServer();
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));
			var member = server.AddMember("Who?", false, Permissions.All);


			var repository = new SimpleUserCommandsRepository();

			var command = new UserCommandInfo("cmd", new NoHandler().InstanceHandler,
				new[] { new UserCommandArgument(false, new[] { UserCommandArgument.Type.Integer }, typeof(int), "arg", SimpleModelAdditionalInfoProvider.Empty) },
				SimpleModelAdditionalInfoProvider.Empty);

			repository.AddCommand(command);


			var pipelineContext = new UserCommandPipelineContext(new UserCommandSendData(member, textChannel), DropRespondHandler);


			var parser = new TextCommandParser(Options.Create(new TextCommandParser.Options() { Prefixes = "*" }), repository);

			//-----------------------------

			var workResult = await parser.ProcessAsync("*cmd 33", pipelineContext);

			//-----------------------------

			Assert.That(workResult.ResultType, Is.EqualTo(UserCommandMiddlewareExcutionResult.Type.Output));

			var context = workResult.GetOutput();
			Assert.That(context.SendData, Is.EqualTo(new UserCommandSendData(member, textChannel)));
			Assert.That(context.Command, Is.EqualTo(command));
			Assert.That(context.Arguments, Is.EquivalentTo(new Dictionary<UserCommandArgument, IReadOnlyList<object>>() { { command.Arguments[0], new object[] { 33 } } }));
		}


		private Task DropRespondHandler(UserCommandResult result)
		{
			Assert.Fail();
			return Task.CompletedTask;
		}
	}
}
