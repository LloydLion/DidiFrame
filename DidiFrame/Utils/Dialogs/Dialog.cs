namespace DidiFrame.Utils.Dialogs
{
	public class Dialog
	{
		private readonly Dictionary<string, IDialogMessage> msgs;
		private readonly DialogStateMachine stateMachine;


		public Dialog(MessageCollection messages, DialogContext ctx, IReadOnlyDictionary<string, IReadOnlyDictionary<string, object?>> usings)
		{
			msgs = usings.ToDictionary(s => s.Key, s => messages.GetMessage(s.Key, ctx, s.Value));
			stateMachine = new DialogStateMachine(msgs);
		}

		public Dialog(MessageCollection messages, DialogContext ctx, IReadOnlyDictionary<string, object> usings) :
			this(messages, ctx, usings.ToDictionary(s => s.Key, s => (IReadOnlyDictionary<string, object?>)s.Value.GetType()
				.GetProperties().ToDictionary(a => a.Name, a => a.GetValue(s.Value)))) { }


		public void Start(string key)
		{
			stateMachine.Start(key);
		}
	}
}
