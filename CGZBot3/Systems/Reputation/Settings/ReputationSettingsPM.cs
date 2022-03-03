namespace CGZBot3.Systems.Reputation.Settings
{
	public class ReputationSettingsPM
	{
		public IReadOnlyCollection<GrantRole> Grants { get; set; } = new List<GrantRole>();

		public Source Sources { get; set; } = new Source();

		public int BanThreshold { get; set; }

		public int GlobalLegalLevelIncrease { get; set; }

		public int GlobalServerActivityDecrease { get; set; }


		public class GrantRole
		{
			public ulong Role { get; set; }

			public int Level { get; set; }

			public ReputationType Type { get; set; }
		}

		public class Source
		{
			public int VoiceCreation { get; set; }

			public int MessageSending { get; set; }
		}
	}
}