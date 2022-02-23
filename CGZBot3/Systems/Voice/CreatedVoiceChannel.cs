namespace CGZBot3.Systems.Voice
{
	public class CreatedVoiceChannel
	{
		public CreatedVoiceChannel(string name, IVoiceChannel baseChannel, IMember creator)
		{
			Name = name;
			BaseChannel = baseChannel;
			Creator = creator;
			StateString = CreatedVoiceChannelLifetime.RunningState;
		}


		public string Name { get; }

		public IVoiceChannel BaseChannel { get; }

		public IMember Creator { get; }

		public string StateString { get; set; }


		public override bool Equals(object? obj)
		{
			return obj is CreatedVoiceChannel channel && channel.BaseChannel.Id == BaseChannel.Id;
		}

		public override int GetHashCode()
		{
			return BaseChannel.Id.GetHashCode();
		}

		public static bool operator ==(CreatedVoiceChannel? left, CreatedVoiceChannel? right)
		{
			return EqualityComparer<CreatedVoiceChannel>.Default.Equals(left, right);
		}

		public static bool operator !=(CreatedVoiceChannel? left, CreatedVoiceChannel? right)
		{
			return !(left == right);
		}
	}
}
