namespace DidiFrame.Data.AutoKeys
{
	/// <summary>
	/// Attribute that can be assigned to data model and associates data key with its. That will be used in auto-key repositories
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class DataKeyAttribute : Attribute
	{
		/// <summary>
		/// Creates new instance of DidiFrame.Data.AutoKeys.DataKeyAttribute and associates key with model
		/// </summary>
		/// <param name="key"></param>
		public DataKeyAttribute(string key)
		{
			ProvidedKey = key;
		}


		/// <summary>
		/// Provided data key for model
		/// </summary>
		public string ProvidedKey { get; }
	}
}
