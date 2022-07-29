namespace DidiFrame.Data.Mongo
{
	/// <summary>
	/// Configuration for mongo database data management
	/// </summary>
	public class DataOptions
	{
		/// <summary>
		/// Option for servers' states
		/// </summary>
		public DataOption? States { get; set; } = null;

		/// <summary>
		/// Option for servers' settings
		/// </summary>
		public DataOption? Settings { get; set; } = null;


		/// <summary>
		/// Configuration for mongo database states or settings management
		/// </summary>
		public class DataOption
		{
			/// <summary>
			/// Connection string to the mongo server
			/// </summary>
			public string ConnectionString { get; set; } = "";

			/// <summary>
			/// Database name on mongo server
			/// </summary>
			public string DatabaseName { get; set; } = "";
		}
	}
}
