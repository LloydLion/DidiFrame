namespace DidiFrame.Data.Json
{
	public class DataOptions
	{
		public DataOption? States { get; set; } = new();

		public DataOption? Settings { get; set; } = new();


		public class DataOption
		{
			public string BaseDirectory { get; set; } = "";
		}
	}
}
