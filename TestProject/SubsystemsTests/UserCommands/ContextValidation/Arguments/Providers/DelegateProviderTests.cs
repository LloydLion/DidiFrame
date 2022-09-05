using DidiFrame.UserCommands.ContextValidation.Arguments.Providers;
using DidiFrame.UserCommands.Models;

namespace TestProject.SubsystemsTests.UserCommands.ContextValidation.Arguments.Providers
{
	public class DelegateProviderTests
	{
		[Test]
		public void ProvideViaDelegate()
		{
			HelpClass.Reset();

			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));

			var sendData = new UserCommandSendData(member, textChannel);

			var provider = new DelegateProvider<int>(typeof(HelpClass), nameof(HelpClass.TestDelegate));

			//------------------

			var providedValues = provider.ProvideValues(sendData);

			//------------------

			Assert.That(providedValues, Is.EquivalentTo(new[] { 11, 14, 13, 12, 15, 16 }));
			Assert.That(HelpClass.LastSendData, Is.EqualTo(sendData));
		}


		private static class HelpClass
		{
			public static UserCommandSendData? LastSendData { get; private set; }


			public static void Reset() { LastSendData = null; }


			public static object[] TestDelegate(UserCommandSendData sendData)
			{
				LastSendData = sendData;
				return new object[] { 11, 14, 13, 12, 15, 16 };
			}
		}
	}
}
