namespace DidiFrame.Utils
{
	/// <summary>
	/// Represents empty service provider that always returns null
	/// </summary>
	public class EmptyServiceProvider : IServiceProvider
	{
		/// <summary>
		/// Cached provider
		/// </summary>
		public static EmptyServiceProvider Instance { get; } = new();


		/// <inheritdoc/>
		public object? GetService(Type serviceType) => null;
	}
}
