using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3
{
	internal class DataBaseContext : DbContext
	{
		private readonly Options options;


		public DataBaseContext(IOptions<Options> options)
		{
			this.options = options.Value;

#if DEBUG
			Database.EnsureDeleted();
			Database.EnsureCreated();
#endif
		}


		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite(options.ConnectionString);
		}

		public class Options
		{
			public string ConnectionString { get; set; } = "";
		}
	}
}
