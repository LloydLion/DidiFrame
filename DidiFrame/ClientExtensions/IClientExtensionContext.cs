namespace DidiFrame.ClientExtensions
{
	public interface IClientExtensionContext<TExtension>
	{
		public void SetExtensionData(object data);

		public object? GetExtensionData();
	}
}
