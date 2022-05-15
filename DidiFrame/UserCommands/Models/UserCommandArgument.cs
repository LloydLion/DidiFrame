using DidiFrame.Utils.ExtendableModels;

namespace DidiFrame.UserCommands.Models
{
	public record UserCommandArgument(bool IsArray, IReadOnlyList<UserCommandArgument.Type> OriginTypes, Type TargetType, string Name, IModelAdditionalInfoProvider AdditionalInfo)
	{
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
