namespace CGZBot3.Systems.Voice
{
	public class ChannelCreationArgsValidator : AbstractValidator<SystemCore.ChannelCreationArgs>
	{
		public ChannelCreationArgsValidator()
		{
			RuleFor(s => s.Name).NotEmpty();
		}
	}
}
