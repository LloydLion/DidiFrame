using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.ClientExtensions
{
	public interface IServerExtensionFactory<TExtension> where TExtension : class
	{
		public Type? TargetServerType { get; }


		public TExtension Create(IServiceProvider services, IServer client);
	}
}
