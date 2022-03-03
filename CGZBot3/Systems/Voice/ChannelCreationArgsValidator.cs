namespace CGZBot3.Systems.Voice
{
	public class ChannelCreationArgsValidator : AbstractValidator<VoiceChannelCreationArgs>
	{
		public ChannelCreationArgsValidator()
		{
			RuleFor(s => s.Name).NotEmpty();
		}
	}
}
