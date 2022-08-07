using System.Reflection;

namespace DidiFrame.UserCommands.ContextValidation.Arguments.Providers
{
	/// <summary>
	/// Provider that delegates its functions
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class DelegateProvider<T> : IUserCommandArgumentValuesProvider
	{
		private readonly MethodInfo methodInfo;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Providers.DelegateProvider`1 using static (public or private) method that collection of objects
		/// </summary>
		/// <param name="type">Type where need to search method</param>
		/// <param name="methodName">Name of provide method</param>
		/// <exception cref="ArgumentException"></exception>
		public DelegateProvider(Type type, string methodName)
		{
			methodInfo = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) ?? throw new ArgumentException("No method found", nameof(methodName));
		}


		/// <inheritdoc/>
		public Type TargetType => typeof(T);


		/// <inheritdoc/>
		public IReadOnlyCollection<object> ProvideValues(UserCommandSendData sendData)
		{
			return (IReadOnlyCollection<object>)(methodInfo.Invoke(null, new object[] { sendData }) ?? throw new NullReferenceException());
		}
	}
}
