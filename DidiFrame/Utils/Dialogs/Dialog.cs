namespace DidiFrame.Utils.Dialogs
{
	public class Dialog
	{
		private readonly Dictionary<string, IDialogMessage> msgs;


		public DialogStateMachine StateMachine { get; }

		public DialogContext Context { get; }


		public Dialog(MessageCollection messages, DialogCreationArgs args, IReadOnlyDictionary<string, IReadOnlyDictionary<string, object?>> usings)
		{
			Context = new DialogContext(args.Channel, args.Invoker, args.LocalizerFactory, this, new());

			msgs = usings.ToDictionary(s => s.Key, s => messages.GetMessage(s.Key, Context, s.Value));

			foreach (var item in usings)
				foreach (var s in item.Value)
					if (s.Value is IDialogOutputParameter parameter)
						parameter.Init(this);

			StateMachine = new DialogStateMachine(msgs);
			StateMachine.MessageSwitched += () =>
			{
				Context.MessageLink.SwapBuffers();
			};
		}

		public Dialog(MessageCollection messages, DialogCreationArgs args, IReadOnlyDictionary<string, object> usings) :
			this(messages, args, usings.ToDictionary(s => s.Key, s => (IReadOnlyDictionary<string, object?>)s.Value.GetType()
				.GetProperties().ToDictionary(a => a.Name, a => a.GetValue(s.Value)))) { }


		public void Start(string key)
		{
			StateMachine.Start(key);
		}

		public void AddFinalizer(Action action)
		{
			StateMachine.DialogFinished += action;
		}
	}
}
