namespace DidiFrame.ClientExtensions
{
	public interface IServerExtensionContext<TExtension>
	{
		public void SetExtensionData(object data);

		public object? GetExtensionData();

		public void SetReleaseCallback(Action callback);
	}
}
