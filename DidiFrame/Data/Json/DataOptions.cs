namespace DidiFrame.Data.Json
{
	/// <summary>
	/// Configuration for json files based data management
	/// </summary>
	public class DataOptions
	{
		/// <summary>
		/// Option for servers' states
		/// </summary>
		public DataOption? States { get; set; } = new();

		/// <summary>
		/// Option for servers' settings
		/// </summary>
		public DataOption? Settings { get; set; } = new();


		/// <summary>
		/// Configuration for json files based states or settings management
		/// </summary>
		public class DataOption
		{
			/// <summary>
			/// Base path for json files
			/// </summary>
			public string BaseDirectory { get; set; } = "";
		}
	}
}
