namespace DidiFrame.Utils.Dialogs
{
	public class MessageCollection
	{
		private readonly IReadOnlyDictionary<string, IDialogMessageFactory> msgs;


		public MessageCollection(IReadOnlyDictionary<string, IDialogMessageFactory> msgs)
		{
			this.msgs = msgs;
		}


		public IDialogMessage GetMessage(string key, DialogContext ctx, IReadOnlyDictionary<string, object?> dynamicParameters) =>
			msgs[key].Create(ctx, dynamicParameters);
	}
}
