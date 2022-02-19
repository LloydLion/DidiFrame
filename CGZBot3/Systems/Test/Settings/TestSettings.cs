namespace CGZBot3.Systems.Test.Settings
{
	internal class TestSettings
	{
		public TestSettings(string someString, ITextChannel testChannel, int dbId)
		{
			SomeString = someString;
			TestChannel = testChannel;
			DatabaseId = dbId;
		}


		public string SomeString { get; init; }

		public ITextChannel TestChannel { get; init; }

		public int DatabaseId { get; }
	}
}
