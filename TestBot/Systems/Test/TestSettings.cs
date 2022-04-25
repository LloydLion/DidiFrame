using DidiFrame.Data.Model;

namespace TestBot.Systems.Test
{
	internal class TestSettings
	{
		public TestSettings(string someString, ITextChannel testChannel)
		{
			SomeString = someString;
			TestChannel = testChannel;
		}


		[ConstructorAssignableProperty(0, "someString")]
		public string SomeString { get; }

		[ConstructorAssignableProperty(1, "testChannel")]
		public ITextChannel TestChannel { get; }
	}
}
