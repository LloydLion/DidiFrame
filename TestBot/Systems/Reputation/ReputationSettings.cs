using DidiFrame.Data.Model;

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


		[ConstructorAssignableProperty(0, "grants")]
		public IReadOnlyCollection<GrantRole> Grants { get; }

		[ConstructorAssignableProperty(1, "sources")]
		public Source Sources { get; }

		[ConstructorAssignableProperty(2, "banThreshold")]
		public int BanThreshold { get; }

		[ConstructorAssignableProperty(3, "globalLegalLevelIncrease")]
		public int GlobalLegalLevelIncrease { get; }

		[ConstructorAssignableProperty(4, "globalServerActivityDecrease")]
		public int GlobalServerActivityDecrease { get; }


		public class GrantRole
		{
			public GrantRole(IRole role, int level, ReputationType type)
			{
				Role = role;
				Level = level;
				Type = type;
			}


			[ConstructorAssignableProperty(0, "role")]
			public IRole Role { get; }

			[ConstructorAssignableProperty(1, "level")]
			public int Level { get; }

			[ConstructorAssignableProperty(2, "type")]
			public ReputationType Type { get; }
		}

		public class Source
		{
			[ConstructorAssignableProperty(0, "voiceCreation")]
			public int VoiceCreation { get; }

			[ConstructorAssignableProperty(1, "messageSending")]
			public int MessageSending { get; }


			public Source(int voiceCreation = 20, int messageSending = 1)
			{
				VoiceCreation = voiceCreation;
				MessageSending = messageSending;
			}
		}
	}
}
