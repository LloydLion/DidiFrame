namespace CGZBot3.Systems.Reputation
{
	public class ReputationSettings
	{
		public ReputationSettings(IReadOnlyCollection<GrantRole> grants, Source sources, int banThreshold = -50, int globalLegalLevelIncrease = 2, int globalServerActivityDecrease = 2)
		{
			Grants = grants;
			Sources = sources;
			BanThreshold = banThreshold;
			GlobalLegalLevelIncrease = globalLegalLevelIncrease;
			GlobalServerActivityDecrease = globalServerActivityDecrease;
		}


		public IReadOnlyCollection<GrantRole> Grants { get; }

		public Source Sources { get; }

		public int BanThreshold { get; }

		public int GlobalLegalLevelIncrease { get; }

		public int GlobalServerActivityDecrease { get; }


		public class GrantRole
		{
			public GrantRole(IRole role, int level, ReputationType type)
			{
				Role = role;
				Level = level;
				Type = type;
			}


			public IRole Role { get; }

			public int Level { get; }

			public ReputationType Type { get; }
		}

		public class Source
		{
			public int VoiceCreation { get; }

			public int MessageSending { get; }


			public Source(int voiceCreation = 20, int messageSending = 1)
			{
				VoiceCreation = voiceCreation;
				MessageSending = messageSending;
			}
		}
	}
}
