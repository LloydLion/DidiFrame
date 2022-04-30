namespace DidiFrame.Utils.Dialogs
{
	public interface IDialogMessageFactory
	{
		public IDialogMessage Create(DialogContext ctx, IReadOnlyDictionary<string, object?> dynamicParameters);
	}
}
