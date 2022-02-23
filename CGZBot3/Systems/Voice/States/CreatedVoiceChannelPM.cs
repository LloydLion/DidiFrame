namespace CGZBot3.Systems.Voice.States
{
	internal class CreatedVoiceChannelPM
	{
		public string Name { get; set; } = "";

		public ulong BaseChannel { get; set; }

		public ulong Creator { get; set; }

		public string StateString { get; set; } = "";
	}
}
