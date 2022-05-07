namespace DidiFrame.Data.AutoKeys
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class DataKeyAttribute : Attribute
	{
		public DataKeyAttribute(string key)
		{
			ProvidedKey = key;
		}


		public string ProvidedKey { get; }
	}
}
