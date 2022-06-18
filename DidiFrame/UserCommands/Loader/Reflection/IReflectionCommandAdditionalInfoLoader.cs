using System.Reflection;

namespace DidiFrame.UserCommands.Loader.Reflection
{
	public interface IReflectionCommandAdditionalInfoLoader
	{
		public IReadOnlyDictionary<Type, object> ProcessArgument(ParameterInfo parameter);

		public IReadOnlyDictionary<Type, object> ProcessMethod(MethodInfo method);
	}
}
