namespace DidiFrame.Data.MongoDB
{
	public class DataOptions
	{
		public DataOption? States { get; set; } = new();

		public DataOption? Settings { get; set; } = new();


		public class DataOption
		{
			public string ConnectionString { get; set; } = "";

			public string DatabaseName { get; set; } = "";
		}
	}
}
