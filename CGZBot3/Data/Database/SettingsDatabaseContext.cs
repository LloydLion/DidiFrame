using CGZBot3.Systems.Test.Settings;
using Microsoft.EntityFrameworkCore;

namespace CGZBot3.Data.Database
{
	internal partial class SettingsDatabaseContext : DbContext
	{
		private readonly DatabaseContextOptions options;


		private DbSet<ServerSettings>? Settings { get; set; }


		public SettingsDatabaseContext(IOptions<DatabaseContextOptions> options) : base()
		{
			this.options = options.Value;

			//Database.EnsureCreated();
		}

		
		public DbSet<ServerSettings> GetSettings()
		{
			return Settings ?? throw new ImpossibleVariantException();
		}


		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite(options.SettingsConnectionString);
		}
	}
}
