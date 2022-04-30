namespace DidiFrame.Utils.Dialogs
{
	public class DialogOutputParameter<T> : IDialogOutputParameter<T> where T : notnull
	{
		private T? value;


		public T GetValue()
		{
			return value ?? throw new InvalidOperationException("No value in output");
		}

		public void Init(Dialog dialog) { }

		public void SetValue(T value)
		{
			if (value is not null)
				throw new InvalidOperationException("Output already setted");

			this.value = value;
		}
	}
}
