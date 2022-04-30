namespace DidiFrame.Utils.Dialogs
{
	public abstract class AbstractMessage : IDialogMessage
	{
		private IMessage? message;


		public AbstractMessage(DialogContext ctx, IReadOnlyDictionary<string, object?> dynamicParamters)
		{
			Context = ctx;
			DynamicParamters = dynamicParamters;
		}


		public bool HasSent { get; }

		public bool IsExist => message?.IsExist ??
			throw new InvalidOperationException("Enable to get existance status when message hasn't showed");

		protected DialogContext Context { get; }

		protected IReadOnlyDictionary<string, object?> DynamicParamters { get; }


		public async Task DeleteAsync()
		{
			if (message is null)
				throw new InvalidOperationException("Enable to delete message when message hasn't sent");

			await message.DeleteAsync();
			message = null;
		}

		public async Task ReshowAsync()
		{
			if (message is null)
				throw new InvalidOperationException("Enable to reshow message when message hasn't sent");

			if (IsExist)
				throw new InvalidOperationException("Enable to reshow message when message still exits");

			message = await CreateMessageAsync();
		}

		public async Task ShowAsync()
		{
			if (message is not null)
				throw new InvalidOperationException("Enable to show message when message has sent");

			message = await CreateMessageAsync();
		}

		protected abstract Task<IMessage> CreateMessageAsync();

		protected T Require<T>(string dynKey)
		{
			return (T)(DynamicParamters[dynKey] ?? throw new ArgumentNullException($"Null is disallowed at [{dynKey}]"));
		}

		protected T? RequireNullable<T>(string dynKey)
		{
			return (T?)DynamicParamters[dynKey];
		}

		protected void NavigateTo(string? msgKey)
		{
			Context.Dialog.StateMachine.SwitchOrFinish(msgKey);
		}

		protected void NavigateToDynamic(string dynKey) => NavigateTo(RequireNullable<string>(dynKey));
	}
}
