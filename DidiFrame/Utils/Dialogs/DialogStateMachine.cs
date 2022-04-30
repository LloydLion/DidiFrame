namespace DidiFrame.Utils.Dialogs
{
	public class DialogStateMachine
	{
		private readonly IReadOnlyDictionary<string, IDialogMessage> msgs;
		private static readonly ThreadLocker<DialogStateMachine> locker = new();


		public DialogStateMachine(IReadOnlyDictionary<string, IDialogMessage> msgs)
		{
			this.msgs = msgs;
		}


		public event Action? DialogFinished;

		public event Action? MessageSwitched;


		public string CurrentMessage { get; private set; } = string.Empty;

		public bool IsRunning => CurrentMessage == string.Empty;


		public async void Start(string key)
		{
			using (locker.Lock(this))
			{
				CurrentMessage = key;
				await msgs[CurrentMessage].ShowAsync();
			}
		}

		public async void Switch(string key)
		{
			using (locker.Lock(this))
			{
				//We must set CurrentMessage property in SYNC mode
				var oldKey = CurrentMessage;
				CurrentMessage = key;
				MessageSwitched?.Invoke();

				await msgs[oldKey].DeleteAsync();
				await msgs[CurrentMessage].ShowAsync();
			}
		}

		public void SwitchOrFinish(string? key)
		{
			if (key is null) Finish();
			else Switch(key);
		}

		public async void Finish()
		{
			using (locker.Lock(this))
			{
				//We must set CurrentMessage property in SYNC mode
				var oldKey = CurrentMessage;
				CurrentMessage = string.Empty;
				DialogFinished?.Invoke();

				await msgs[oldKey].DeleteAsync();
			}
		}
	}
}
