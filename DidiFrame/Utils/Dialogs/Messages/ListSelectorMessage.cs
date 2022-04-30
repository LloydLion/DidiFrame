using DidiFrame.Entities.Message;
using DidiFrame.Entities.Message.Components;

namespace DidiFrame.Utils.Dialogs.Messages
{
	public class ListSelectorMessage<TList> : AbstractMessage where TList : class
	{
		public ListSelectorMessage(DialogContext ctx, IReadOnlyDictionary<string, object?> dynamicParamters) : base(ctx, dynamicParamters) { }


		protected async override Task<IMessage> CreateMessageAsync()
		{
			var objs = Require<IReadOnlyList<TList>>("List");
			var stringifier = RequireNullable<Func<TList, string>>("ToString") ?? new((obj) => obj.ToString() ?? throw new ArgumentNullException());
			var output = Require<IDialogOutputParameter<TList>>("Output");
			var localizer = Context.LocalizerFactory.Create(typeof(ListSelectorMessage<>));


			var model = new MessageSendModel()
			{
				ComponentsRows = new[]
				{
					new MessageComponentsRow(new []
					{
						new MessageSelectMenu("menu", objs.Select((s, i) => new MessageSelectMenuOption(stringifier(s), i.ToString())).ToArray(), "I don't what it is")
					})
				}
			};


			var msg = await Context.Channel.SendMessageAsync(model);


			var di = msg.GetInteractionDispatcher();
			di.Attach<MessageSelectMenu>("menu", (ctx) =>
			{
				var selectedOption = ((MessageSelectMenuState)(ctx.ComponentState ?? throw new ImpossibleVariantException())).SelectedValues.Single();
				output.SetValue(objs[int.Parse(selectedOption)]);

				NavigateToDynamic("NextMessage");

				return Task.FromResult(new ComponentInteractionResult(new MessageSendModel(localizer["MenuClickMessage"])));
			});


			return msg;
		}
	}
}
