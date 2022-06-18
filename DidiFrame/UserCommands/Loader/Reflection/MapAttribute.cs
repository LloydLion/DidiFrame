using DidiFrame.UserCommands.ContextValidation;
using DidiFrame.Utils;

namespace DidiFrame.UserCommands.Loader.Reflection
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = false)]
    public class MapAttribute : Attribute
	{
        public const string ProviderErrorCode = ContextValidator.ProviderErrorCode;


		private readonly LocaleMap map;


		public MapAttribute(params (string, string)[] pairs)
		{
			var preMap = new Dictionary<string, string>();

			foreach (var pair in pairs) preMap.Add(pair.Item1, pair.Item2);

			map = new(preMap);
		}


		public LocaleMap GetLocaleMap() => map;
	}
}
