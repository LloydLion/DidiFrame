using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Culture
{
	internal class SettingsConverter : ISettingsConverter<CultureSettingsPM, CultureSettings>
	{
		public Task<CultureSettingsPM> ConvertDownAsync(IServer server, CultureSettings origin)
		{
			return Task.FromResult(new CultureSettingsPM() { CultureKey = origin.CultureInfo.Name });
		}

		public Task<CultureSettings> ConvertUpAsync(IServer server, CultureSettingsPM pm)
		{
			return Task.FromResult(new CultureSettings(new CultureInfo(pm.CultureKey)));
		}
	}
}
