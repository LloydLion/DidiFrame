using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.UserCommands.Loader.Reflection
{
	/// <summary>
	/// Indicates method as command
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class CommandAttribute : Attribute
	{
		/// <summary>
		/// Creates new instance of CommandAttribute
		/// </summary>
		/// <param name="name">Name of new command</param>
		public CommandAttribute(string name)
		{
			Name = name;
		}

		/// <summary>
		/// Creates new instance of CommandAttribute in auto return mode
		/// </summary>
		/// <param name="name">Name of new command</param>
		/// <param name="returnLocaleKey">Locale key of result that will be transcripted by default localizer and auto returned, method can return void or task</param>
		public CommandAttribute(string name, string returnLocaleKey)
		{
			Name = name;
			ReturnLocaleKey = returnLocaleKey;
		}


		/// <summary>
		/// Name of new command
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Locale key of result or null if command hasn't want use result auto return
		/// </summary>
		public string? ReturnLocaleKey { get; }
	}
}
