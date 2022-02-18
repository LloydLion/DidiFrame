using Microsoft.EntityFrameworkCore;

namespace CGZBot3.Data.Database
{
	internal class StateDatabaseContext : DbContext
	{
		private readonly DatabaseContextOptions options;


		private DbSet<ServerState>? GlobalState { get; set; }


		public StateDatabaseContext(IOptions<DatabaseContextOptions> options) : base()
		{
			this.options = options.Value;

			//Database.EnsureCreated();
		}


		public DbSet<ServerState> GetGlobalState()
		{
			return GlobalState ?? throw new ImpossibleVariantException();
		}


		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite(options.StateConnectionString);
		}
	}
}
