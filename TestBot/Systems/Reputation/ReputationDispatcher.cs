﻿namespace TestBot.Systems.Reputation
{
	public class ReputationDispatcher : IReputationDispatcher, IDisposable
	{
		private readonly Voice.ISystemNotifier voiceNotifier;
		private readonly IClient client;
		private readonly IMembersReputationRepository repository;
		private readonly IServersSettingsRepository<ReputationSettings> settings;
		private Task? legalLevelIncreeseTask;
		private readonly CancellationTokenSource cts;


		public event Action<MemberReputation>? ReputationChanged;


		public ReputationDispatcher(
			Voice.ISystemNotifier voiceNotifier,
			IClient client,
			IMembersReputationRepository repository,
			IServersSettingsRepositoryFactory settingsFactory)
		{
			this.voiceNotifier = voiceNotifier;
			this.client = client;
			this.repository = repository;
			settings = settingsFactory.Create<ReputationSettings>(SettingsKeys.ReputationSystem);
			cts = new CancellationTokenSource();
		}


		public void Start()
		{
			legalLevelIncreeseTask = CreatePriodicRpUpdateTask(cts.Token);
			voiceNotifier.ChannelCreated += VoiceCreated;

			client.ServerCreated += ServerCreated; //Adds message sent event handler for each new server
			foreach (var server in client.Servers) server.MessageSent += MessageSent;
		}

		private void VoiceCreated(object? _, Voice.VoiceChannelCreatedEventArgs args)
		{
			using var rp = repository.GetReputation(args.Owner);
			
			var setting = settings.Get(args.Owner.Server);

			rp.Object.Increase(ReputationType.ServerActivity, setting.Sources.VoiceCreation);
			rp.Object.Increase(ReputationType.Experience, setting.Sources.VoiceCreation);
			ReputationChanged?.Invoke(rp.Object);
		}

		private void MessageSent(IClient _, IMessage message, bool isModified)
		{
			if (isModified) return;

			using var rp = repository.GetReputation(message.Author);

			var setting = settings.Get(message.TextChannel.Server);

			rp.Object.Increase(ReputationType.ServerActivity, setting.Sources.MessageSending);
			rp.Object.Increase(ReputationType.Experience, setting.Sources.MessageSending);
			ReputationChanged?.Invoke(rp.Object);
		}

		public void ServerCreated(IServer server)
		{
			server.MessageSent += MessageSent;
		}

		private async Task CreatePriodicRpUpdateTask(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				//Process only at 00:00

				var now = DateTime.Now;
				var yesterday = DateOnly.FromDateTime(now).AddDays(1).ToDateTime(new TimeOnly(0, 0)); //Set time here
				var ticks = ((int)(yesterday - now).TotalMilliseconds) - 10000;
				await Task.Delay(ticks, token);
				if (token.IsCancellationRequested) break;

				var servers = client.Servers;

				foreach (var server in servers) ProcessServerPeriodicTasks(server);
			}
		}

		private void ProcessServerPeriodicTasks(IServer server)
		{
			var members = server.GetMembers();

			var setting = settings.Get(server);

			foreach (var member in members)
			{
				using var rp = repository.GetReputation(member);

				var ll = rp.Object.Reputation[ReputationType.LegalLevel];
				if (ll < 0)	rp.Object.Increase(ReputationType.LegalLevel, Math.Min(setting.GlobalLegalLevelIncrease, -ll));
				
				var sa = rp.Object.Reputation[ReputationType.ServerActivity];
				if (sa > 0) rp.Object.Decrease(ReputationType.ServerActivity, Math.Min(setting.GlobalServerActivityDecrease, sa));

				ReputationChanged?.Invoke(rp.Object);
			}
		}

		public void Dispose()
		{
			cts.Cancel();
			legalLevelIncreeseTask?.Wait();
			legalLevelIncreeseTask?.Dispose();

			//Unsubscribe
			voiceNotifier.ChannelCreated -= VoiceCreated;
			client.ServerCreated -= ServerCreated;

			GC.SuppressFinalize(this);
		}
	}
}
