using DidiFrame.UserCommands.ContextValidation;
using DidiFrame.UserCommands.Loader.Reflection;
using DidiFrame.Utils;
using System.Reflection;

namespace TestBot.Systems.Test
{
	internal class LazyAdditionalLoader : IReflectionCommandAdditionalInfoLoader
	{
		public IReadOnlyDictionary<Type, object> ProcessArgument(ParameterInfo parameter)
		{
			return new Dictionary<Type, object>
			{
				{ typeof(LocaleMap), new LocaleMap(new Dictionary<string, string>() { { ContextValidator.ProviderErrorCode, "ProviderError" } }) }
			};
		}

		public IReadOnlyDictionary<Type, object> ProcessMethod(MethodInfo method)
		{
			return new Dictionary<Type, object>();
		}
	}
}
