namespace CGZBot3.UserCommands.Loader.Reflection
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	internal class CommandAttribute : Attribute
	{
		public CommandAttribute(string name)
		{
			Name = name;
		}


		public string Name { get; }
	}
}
