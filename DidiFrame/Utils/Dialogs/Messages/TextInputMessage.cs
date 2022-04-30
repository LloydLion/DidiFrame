using DidiFrame.Entities.Message;

namespace DidiFrame.Utils.Dialogs.Messages
{
	public class TextInputMessage<TOutput> : AbstractMessage where TOutput : notnull
	{
		public TextInputMessage(DialogContext ctx, IReadOnlyDictionary<string, object?> dynamicParamters) : base(ctx, dynamicParamters) { }


		protected async override Task<IMessage> CreateMessageAsync()
		{
			MessageSendModel model;

			if (DynamicParamters.ContainsKey("Text") == false) model = Require<MessageSendModel>("Model");
			else model = new(Require<string>("Text"));


			var msg = await Context.Channel.SendMessageAsync(model);
			Context.Channel.MessageSent += TextChannel_MessageSent;


			return msg;
		}


		private void TextChannel_MessageSent(IClient sender, IMessage message)
		{
			var parser = typeof(TOutput) == typeof(string) ? new Func<string, TOutput>((str) => (TOutput)(object)str) : Require<Func<string, TOutput>>("Parser");
			var controller = Require<Func<string, bool>>("Controller");

			if (message.Content is not null && controller(message.Content))
			{
				message.TextChannel.MessageSent -= TextChannel_MessageSent;

				var obj = parser(message.Content);

				Require<IDialogOutputParameter<TOutput>>("Output").SetValue(obj);

				NavigateToDynamic("NextMessage");
			}
		}
	}
}
