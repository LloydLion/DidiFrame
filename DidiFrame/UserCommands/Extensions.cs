namespace DidiFrame.UserCommands
{
	/// <summary>
	/// Extensions for DidiFrame.UserCommands namespace
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Gets target type from user command argumnet type
		/// </summary>
		/// <param name="type">User command argumnet type</param>
		/// <returns>Target type</returns>
		/// <exception cref="NotSupportedException">If method don't support input type, if type is default method is obsolete</exception>
		public static Type GetReqObjectType(this UserCommandArgument.Type type)
		{
			return type switch
			{
				UserCommandArgument.Type.Integer => typeof(int),
				UserCommandArgument.Type.Double => typeof(double),
				UserCommandArgument.Type.String => typeof(string),
				UserCommandArgument.Type.Member => typeof(IMember),
				UserCommandArgument.Type.Role => typeof(IRole),
				UserCommandArgument.Type.Mentionable => typeof(object),
				UserCommandArgument.Type.TimeSpan => typeof(TimeSpan),
				UserCommandArgument.Type.DateTime => typeof(DateTime),
				_ => throw new NotSupportedException(),
			};
		}
	}
}
