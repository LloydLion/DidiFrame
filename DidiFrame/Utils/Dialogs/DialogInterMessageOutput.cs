namespace DidiFrame.Utils.Dialogs
{
	public class DialogInterMessageOutput<TModel> : IDialogOutputParameter<TModel> where TModel : notnull
	{
		private readonly string interKey;
		private readonly bool toGlobal;
		private Dialog? dialog;


		public DialogInterMessageOutput(string interKey, bool toGlobal = false)
		{
			this.interKey = interKey;
			this.toGlobal = toGlobal;
		}


		public void Init(Dialog dialog) => this.dialog = dialog;

		public void SetValue(TModel value)
		{
			if (toGlobal)
				dialog?.Context.MessageLink.SetGlobalVariable(interKey, value);
			else
				dialog?.Context.MessageLink.SetChainedVariable(interKey, value);
		}
	}
}
