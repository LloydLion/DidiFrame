namespace CGZBot3.Systems.Test.Settings
{
	internal class TestSettings
	{
		public TestSettings(string someString, ITextChannel testChannel)
		{
			SomeString = someString;
			TestChannel = testChannel;
		}


		public string SomeString { get; }

		public ITextChannel TestChannel { get; }
	}
}
