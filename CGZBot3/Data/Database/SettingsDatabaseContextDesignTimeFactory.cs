using Microsoft.EntityFrameworkCore.Design;

namespace CGZBot3.Data.Database
{
	internal class SettingsDatabaseContextDesignTimeFactory : IDesignTimeDbContextFactory<SettingsDatabaseContext>
	{
		public SettingsDatabaseContext CreateDbContext(string[] args)
		{
			return new SettingsDatabaseContext(Options.Create(new DatabaseContextOptions() { SettingsConnectionString = string.Join(' ', args) }));
		}
	}
}
