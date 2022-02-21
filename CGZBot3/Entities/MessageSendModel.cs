using CGZBot3.Entities.Embed;

namespace CGZBot3.Entities
{
	public record MessageSendModel(string Content)
	{
		public MessageEmbed? MessageEmbed { get; init; }

		public IReadOnlyCollection<MessageFile>? Files { get; init; }
	}
}