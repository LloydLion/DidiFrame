using DidiFrame.Dependencies;

namespace DidiFrame.UserCommands.PreProcessing
{
	/// <summary>
	/// Instance creator that creates sub converter using dynamic analyzing and dependency injecting
	/// </summary>
	/// <typeparam name="TConverter"></typeparam>
	public class ReflectionContextSubConverterInstanceCreator<TConverter> : IContextSubConverterInstanceCreator where TConverter : IUserCommandContextSubConverter
	{
		private readonly IServiceProvider services;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.PreProcessing.ReflectionContextSubConverterInstanceCreator`1
		/// </summary>
		/// <param name="services">Services to resolve sub converters dependencies</param>
		public ReflectionContextSubConverterInstanceCreator(IServiceProvider services)
		{
			this.services = services;
		}


		/// <inheritdoc/>
		public IUserCommandContextSubConverter Create() => services.ResolveDependencyObject<TConverter>();
	}
}
