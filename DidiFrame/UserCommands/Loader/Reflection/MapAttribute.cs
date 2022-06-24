using DidiFrame.UserCommands.ContextValidation;
using DidiFrame.Utils;

namespace DidiFrame.UserCommands.Loader.Reflection
{
	/// <summary>
	/// Map attribute that adds locale map for values providers, validators and filters
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = false)]
    public class MapAttribute : Attribute
	{
		/// <summary>
		/// Error code that will be transcipted if a provider give error
		/// </summary>
		public const string ProviderErrorCode = ContextValidator.ProviderErrorCode;


		private readonly LocaleMap map;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Loader.Reflection.MapAttribute
		/// </summary>
		/// <param name="pairs">Pairs that will used to create locale map. Elemnt count must be even</param>
		public MapAttribute(params string[] pairs)
		{
			var preMap = new Dictionary<string, string>();

			for (int i = 0; i < pairs.Length; i += 2)
			{
				preMap.Add(pairs[i], pairs[i + 1]);
			}

			map = new(preMap);
		}


		/// <summary>
		/// Creates locale map that based on attibute
		/// </summary>
		/// <returns></returns>
		public LocaleMap GetLocaleMap() => map;
	}
}
