using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DidiFrame.AutoInjecting
{
	/// <summary>
	/// Injects dependencies using instance of classes that implements DidiFrame.AutoInjecting.IAutoSubinjector interface in given assembly
	/// </summary>
	public class ReflectionAutoInjector : IAutoInjector
	{
		private readonly Assembly targetAssembly;


		/// <summary>
		/// Creates a new instance of DidiFrame.AutoInjecting.ReflectionAutoInjector for given assembly
		/// </summary>
		/// <param name="targetAssembly">Assembly for type finding</param>
		public ReflectionAutoInjector(Assembly targetAssembly)
		{
			this.targetAssembly = targetAssembly;
		}

		/// <summary>
		/// Creates a new instance of DidiFrame.AutoInjecting.ReflectionAutoInjector for calling assembly
		/// </summary>
		public ReflectionAutoInjector() : this(Assembly.GetCallingAssembly()) { }


		/// <inheritdoc/>
		public void InjectDependencies(IServiceCollection services)
		{
			var subinjectors = targetAssembly.GetTypes().Where(s => s.GetInterfaces().Contains(typeof(IAutoSubInjector))).ToArray();
			foreach (var subi in subinjectors)
			{
				((IAutoSubInjector)(Activator.CreateInstance(subi) ?? throw new ImpossibleVariantException())).InjectDependencies(services);
			}
		}
	}
}
