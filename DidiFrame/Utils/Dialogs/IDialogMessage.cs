namespace DidiFrame.Utils.Dialogs
{
	public interface IDialogMessage
	{
		public bool HasSent { get; }

		public bool IsExist { get; }


		public Task ShowAsync();

		public Task ReshowAsync();

		public Task DeleteAsync();
	}
}
