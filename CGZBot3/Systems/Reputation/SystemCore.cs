using CGZBot3.GlobalEvents;

namespace CGZBot3.Systems.Reputation
{
	public class SystemCore : IDisposable
	{
		private readonly static EventId ManageGrantsErrorID = new(33, "ManageGrantsError");


		private readonly IServersSettingsRepository<ReputationSettings> settings;
		private readonly IMembersReputationRepository repository;
		private readonly IValidator<MemberLegalLevelChangeOperationArgs> argsValidator;
		private readonly IReputationDispatcher dispatcher;
		private readonly ILogger<SystemCore> logger;
		private readonly StartupEvent startup;


		public SystemCore(
			IServersSettingsRepositoryFactory settingsFactory,
			IMembersReputationRepository repository,
			IValidator<MemberLegalLevelChangeOperationArgs> argsValidator,
			IReputationDispatcher dispatcher,
			ILogger<SystemCore> logger,
			StartupEvent startup)
		{
			settings = settingsFactory.Create<ReputationSettings>(SettingsKeys.ReputationSystem);
			this.repository = repository;
			this.argsValidator = argsValidator;
			this.dispatcher = dispatcher;
			this.logger = logger;
			this.startup = startup;
			startup += Startup;
		}


		private void Startup()
		{
			dispatcher.Start();
			dispatcher.ReputationChanged += OnReputationChanged;
		}

		private async void OnReputationChanged(MemberReputation rp)
		{
			//Manage grants

			var grants = settings.Get(rp.Member.Server).Grants;

			IReadOnlyCollection<IRole> roles;
			try { roles = rp.Member.GetRoles(); }
			catch (Exception ex) { logger.Log(LogLevel.Warning, ManageGrantsErrorID, ex, "Enable to get member roles"); return; }

			var tasks = new List<Task>();

			foreach (var grant in grants)
			{
				var count = rp.Reputation[grant.Type];

				if (grant.Type == ReputationType.LegalLevel)
				{
					if (count <= grant.Level && !roles.Contains(grant.Role)) //can and not contains, add
						tasks.Add(rp.Member.GrantRoleAsync(grant.Role));
					else if (count > grant.Level && roles.Contains(grant.Role)) //can't but contains, delete
						tasks.Add(rp.Member.RevokeRoleAsync(grant.Role));
				}
				else
				{
					if (count >= grant.Level && !roles.Contains(grant.Role)) //can and not contains, add
						tasks.Add(rp.Member.GrantRoleAsync(grant.Role));
					else if (count < grant.Level && roles.Contains(grant.Role)) //can't but contains, delete
						tasks.Add(rp.Member.RevokeRoleAsync(grant.Role));
				}
			}

			try { await Task.WhenAll(tasks); }
			catch (Exception ex) { logger.Log(LogLevel.Warning, ManageGrantsErrorID, ex, "Enable to apply roles to member"); return; }
		}

		public bool AddIllegal(MemberLegalLevelChangeOperationArgs args)
		{
			argsValidator.ValidateAndThrow(args);

			using var rp = repository.GetReputation(args.Member);
			var setting = settings.Get(args.Member.Server);

			rp.Object.Decrease(ReputationType.LegalLevel, args.Amount);

			return rp.Object.Reputation[ReputationType.LegalLevel] < setting.BanThreshold;
		}

		public void Dispose()
		{
			_ = startup - Startup;
			dispatcher.ReputationChanged -= OnReputationChanged;

			GC.SuppressFinalize(this);
		}

		public IReadOnlyDictionary<ReputationType, int> GetReputation(IMember member)
		{
			using var rp = repository.GetReputation(member);

			return rp.Object.Reputation;
		}
	}
}
