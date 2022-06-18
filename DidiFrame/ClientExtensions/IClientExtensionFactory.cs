using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.ClientExtensions
{
	public interface IClientExtensionFactory<TExtension> where TExtension : class
	{
		public Type? TargetClientType { get; }


		public TExtension Create(IServiceProvider services, IClient client);
	}
}
