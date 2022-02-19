using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CGZBot3.Systems.Test.Settings
{
	internal class TestSettingsDB
	{
		public int Id { get; set; }

		public string SomeString { get; set; } = "";

		public ulong TestChannel { get; set; }
	}
}
