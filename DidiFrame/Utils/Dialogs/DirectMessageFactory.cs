namespace DidiFrame.Utils.Dialogs
{
	public class DirectMessageFactory<TMessage> : IDialogMessageFactory where TMessage : IDialogMessage
	{
		public IDialogMessage Create(DialogContext ctx, IReadOnlyDictionary<string, object?> dynamicParameters)
		{
			return (IDialogMessage)(Activator.CreateInstance(typeof(TMessage), ctx, dynamicParameters) ?? throw new ImpossibleVariantException());
		}
	}
}
