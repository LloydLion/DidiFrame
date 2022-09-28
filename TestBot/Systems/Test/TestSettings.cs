using DidiFrame.Data.Model;

namespace TestBot.Systems.Test
{
	internal class TestSettings : IDataModel
	{
		[SerializationConstructor]
		public TestSettings(string someString, ITextChannel testChannel)
		{
			SomeString = someString;
			TestChannel = testChannel;
		}


		[ConstructorAssignableProperty(0, "someString")]
		public string SomeString { get; }

		[ConstructorAssignableProperty(1, "testChannel")]
		public ITextChannel TestChannel { get; }

		public Guid Id { get; } = Guid.NewGuid();


		public bool Equals(IDataModel? other) => other is TestSettings settings && Equals(settings.TestChannel.Server, TestChannel.Server);

		public override bool Equals(object? obj) => Equals(obj as IDataModel);

		public override int GetHashCode() => TestChannel.Server.GetHashCode();
	}
}
