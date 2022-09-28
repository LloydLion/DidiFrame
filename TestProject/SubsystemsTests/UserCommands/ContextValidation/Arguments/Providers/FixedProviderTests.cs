using DidiFrame.UserCommands.ContextValidation.Arguments.Providers;
using DidiFrame.UserCommands.Models;

namespace TestProject.SubsystemsTests.UserCommands.ContextValidation.Arguments.Providers
{
	public class FixedProviderTests
	{
		[Test]
		public void ProvideFixedViaCtor()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));

			var sendData = new UserCommandSendData(member, textChannel);

			var provider = new FixedProvider<int>(11, 15, 12, 13, 14, 15, 16, 17, 18, 19);

			//------------------

			var providedValues = provider.ProvideValues(sendData);

			//------------------

			Assert.That(providedValues, Is.EquivalentTo(new[] { 11, 15, 12, 13, 14, 15, 16, 17, 18, 19 }));
		}

		[Test]
		public void ProvideFixedViaCollection()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));

			var sendData = new UserCommandSendData(member, textChannel);

			var provider = new FixedProvider<int>(new int[] { 11, 15, 12, 13, 14, 15, 16, 17, 18, 19 });

			//------------------

			var providedValues = provider.ProvideValues(sendData);

			//------------------

			Assert.That(providedValues, Is.EquivalentTo(new[] { 11, 15, 12, 13, 14, 15, 16, 17, 18, 19 }));
		}
	}
}
