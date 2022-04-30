namespace DidiFrame.Utils.Dialogs
{
	public class PrimitiveMessage : IDialogMessage
	{
		private readonly DialogContext ctx;
		private readonly Func<DialogContext, Task<IMessage>> msgCreator;
		private IMessage? message;


		public PrimitiveMessage(DialogContext ctx, Func<DialogContext, Task<IMessage>> msgCreator)
		{
			this.ctx = ctx;
			this.msgCreator = msgCreator;
		}


		public bool HasSent { get; }

		public bool IsExist => message?.IsExist ??
			throw new InvalidOperationException("Enable to get existance status when message hasn't showed");


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

			message = await msgCreator(ctx);
		}

		public async Task ShowAsync()
		{
			if (message is not null)
				throw new InvalidOperationException("Enable to show message when message has sent");

			message = await msgCreator(ctx);
		}
	}
}
