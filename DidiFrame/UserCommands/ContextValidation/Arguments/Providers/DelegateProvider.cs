using System.Reflection;

namespace DidiFrame.UserCommands.ContextValidation.Arguments.Providers
{
	public class DelegateProvider<T> : IUserCommandArgumentValuesProvider
	{
		private readonly MethodInfo methodInfo;


		public DelegateProvider(Type type, string methodName)
		{
			methodInfo = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) ?? throw new ArgumentException(nameof(methodName), "No method found");
		}


		/// <inheritdoc/>
		public Type TargetType => typeof(T);


		/// <inheritdoc/>
		public IReadOnlyCollection<object> ProvideValues(IServer server, IServiceProvider services)
		{
			return (IReadOnlyCollection<object>)(methodInfo.Invoke(null, new object[] { server, services }) ?? throw new NullReferenceException());
		}
	}
}
