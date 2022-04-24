namespace DidiFrame.UserCommands.Loader.Reflection
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class CommandAttribute : Attribute
	{
		public CommandAttribute(string name)
		{
			Name = name;
		}


		public string Name { get; }
	}
}
