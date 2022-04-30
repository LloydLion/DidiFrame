namespace DidiFrame.Utils.Dialogs
{
	public interface IDialogOutputParameter<in T> : IDialogOutputParameter where T : notnull
	{
		public void SetValue(T value);
	}

	public interface IDialogOutputParameter
	{
		public void Init(Dialog dialog);
	}
}
