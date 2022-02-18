using Microsoft.EntityFrameworkCore.Design;

namespace CGZBot3.Data.Database
{
	internal class StateDatabaseContextDesignTimeFactory : IDesignTimeDbContextFactory<StateDatabaseContext>
	{
		public StateDatabaseContext CreateDbContext(string[] args)
		{
			return new StateDatabaseContext(Options.Create(new DatabaseContextOptions() { StateConnectionString = string.Join(' ', args) }));
		}
	}
}
