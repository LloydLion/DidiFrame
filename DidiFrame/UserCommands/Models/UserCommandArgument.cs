using DidiFrame.Utils.ExtendableModels;

namespace DidiFrame.UserCommands.Models
{
	/// <summary>
	/// Represents user command argument information
	/// </summary>
	/// <param name="IsArray">If argument is array of primitives</param>
	/// <param name="OriginTypes">Original types if target type is complex else one type</param>
	/// <param name="TargetType">Excepted type of argument</param>
	/// <param name="Name">Calling name of argument</param>
	/// <param name="AdditionalInfo">A model additional info provider to provide additional and dynamic data about model</param>
	public record UserCommandArgument(bool IsArray, IReadOnlyList<UserCommandArgument.Type> OriginTypes, Type TargetType, string Name, IModelAdditionalInfoProvider AdditionalInfo)
	{
		/// <summary>
		/// Type of pre object of user command argument
		/// </summary>
		public enum Type
		{
			Integer,
			Double,
			String,
			Member,
			Role,
			Mentionable,
			TimeSpan,
			DateTime
		}
	}
}
