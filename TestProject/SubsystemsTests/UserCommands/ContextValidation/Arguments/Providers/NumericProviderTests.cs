using DidiFrame.UserCommands.ContextValidation.Arguments.Providers;
using DidiFrame.UserCommands.Models;

namespace TestProject.SubsystemsTests.UserCommands.ContextValidation.Arguments.Providers
{
	public class NumericProviderTests
	{
		[Test]
		public void ProvideNumbers()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));

			var sendData = new UserCommandSendData(member, textChannel);

			var provider = new NumericProvider(11, 15);

			//------------------

			var providedValues = provider.ProvideValues(sendData);

			//------------------

			Assert.That(providedValues, Is.EquivalentTo(new[] { 11, 12, 13, 14, 15 }));
		}
	}
}
