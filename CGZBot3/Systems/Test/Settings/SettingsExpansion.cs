using CGZBot3.Systems.Test.Settings;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace CGZBot3.Data
{
	internal partial class ServerSettings
	{
		public virtual TestSettingsDB? TestSystem { get; set; }

		[ForeignKey(nameof(TestSystem))]
		public int? TestSystemId { get; set; }
	}

	namespace Database
	{
		internal partial class SettingsDatabaseContext
		{
			public DbSet<TestSettingsDB>? TestSettings { get; set; }
		}
	}
}
