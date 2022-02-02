using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3
{
	internal class DataBaseContext : DbContext
	{
		private static string? connectionString;


		public DataBaseContext()
		{

		}


		public static void AddSettings(IConfiguration configuration)
		{
			connectionString = configuration.GetValue<string>("connectionString");
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite(connectionString ?? throw new InvalidOperationException
				("Please setup a configuration with static method AddSettings before create instances"));
		}
	}
}
