namespace CGZBot3.Systems.Reputation
{
	public class ReputationDispatcher : IReputationDispatcher, IDisposable
	{
		private readonly Voice.ISystemNotifier voiceNotifier;
		private readonly IClient client;
		private readonly IMembersReputationRepository repository;
		private readonly ISettingsRepository settings;
		private Task? legalLevelIncreeseTask;
		private readonly CancellationTokenSource cts;


		public event Action<MemberReputation>? ReputationChanged;


		public ReputationDispatcher(
			Voice.ISystemNotifier voiceNotifier,
			IClient client,
			IMembersReputationRepository repository,
			ISettingsRepository settings)
		{
			this.voiceNotifier = voiceNotifier;
			this.client = client;
			this.repository = repository;
			this.settings = settings;
			cts = new CancellationTokenSource();
		}


		public void Start()
		{
			legalLevelIncreeseTask = CreateLegalIncreeseTask(cts.Token);
			voiceNotifier.ChannelCreated += VoiceCreated;
			client.MessageSent += MessageSent;
		}


		private async void VoiceCreated(object? _, Voice.VoiceChannelCreatedEventArgs args)
		{
			await using var rp = repository.GetReputation(args.CreationArgs.Owner);

			var setting = settings.GetSettings(args.Lifetime.BaseObject.BaseChannel.Server);

			rp.Object.Increase(ReputationType.ServerActivity, setting.Sources.VoiceCreation);
			rp.Object.Increase(ReputationType.Experience, setting.Sources.VoiceCreation);
			ReputationChanged?.Invoke(rp.Object);
		}

		private async void MessageSent(IClient _, IMessage message)
		{
			await using var rp = repository.GetReputation(message.Author);

			var setting = settings.GetSettings(message.TextChannel.Server);

			rp.Object.Increase(ReputationType.ServerActivity, setting.Sources.MessageSending);
			rp.Object.Increase(ReputationType.Experience, setting.Sources.MessageSending);
			ReputationChanged?.Invoke(rp.Object);
		}

		private async Task CreateLegalIncreeseTask(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				//Increese only at 00:00

				var now = DateTime.Now;
				var yesterday = DateOnly.FromDateTime(now).AddDays(1).ToDateTime(new TimeOnly(0, 0)); //Set time here
				var ticks = ((int)(yesterday - now).TotalMilliseconds) - 10000;
				await Task.Delay(ticks, token);
				if (token.IsCancellationRequested) break;

				var servers = client.Servers;

				var tasks = new List<Task>();

				foreach (var server in servers) tasks.Add(ProcessServerPeriodicTasks(server));

				await Task.WhenAll(tasks);
			}
		}

		private async Task ProcessServerPeriodicTasks(IServer server)
		{
			var members = await server.GetMembersAsync();

			var setting = settings.GetSettings(server);

			foreach (var member in members)
			{
				await using var rp = repository.GetReputation(member);

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
			client.MessageSent -= MessageSent;

			GC.SuppressFinalize(this);
		}
	}
}
