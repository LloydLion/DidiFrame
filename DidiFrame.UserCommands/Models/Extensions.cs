using System.Reflection;

namespace DidiFrame.UserCommands.Models
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
			return type.GetType().GetField(type.ToString())?.GetCustomAttribute<IndicateTypeAttribute>()?.Type ?? throw new ImpossibleVariantException();
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	internal class IndicateTypeAttribute : Attribute
	{
		public IndicateTypeAttribute(Type type)
		{
			Type = type;
		}


		public Type Type { get; }
	}
}
