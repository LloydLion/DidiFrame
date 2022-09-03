using DidiFrame.UserCommands.ContextValidation.Arguments.Providers;
using DidiFrame.UserCommands.Models;

namespace TestProject.SubsystemsTests.UserCommands.ContextValidation.Arguments.Providers
{
	public class DelegateProviderTests
	{
		[Test]
		public void ProvideViaDelegate()
		{
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
			Assert.That(HelpClass.TestDelegateInvokingCount, Is.EqualTo(1));
		}


		private static class HelpClass
		{
			public static int TestDelegateInvokingCount { get; private set; }


			public static void Reset() { TestDelegateInvokingCount = 0; }


			public static object[] TestDelegate()
			{
				TestDelegateInvokingCount++;
				return new object[] { 11, 14, 13, 12, 15, 16 };
			}
		}
	}
}
