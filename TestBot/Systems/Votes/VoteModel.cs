using DidiFrame.Data.AutoKeys;
using DidiFrame.Lifetimes;
using DidiFrame.Data.Model;
using DidiFrame.Utils;

namespace TestBot.Systems.Votes
{
	[DataKey("vote")]
	internal class VoteModel : AbstractModel, ILifetimeBase
	{
		public VoteModel(IMember creator, IReadOnlyList<string> options, string title, ITextChannelBase channel)
		{
			Options = options.ToDictionary(s => s, s => 0);
			Creator = creator;
			Title = title;
			Message = new(channel);
		}

#nullable disable
		public VoteModel(ISerializationModel model) : base(model) { }
#nullable restore


		[ModelProperty(PropertyType.Primitive)]
		public IMember Creator { get => GetDataFromStore<IMember>(); private set => SetDataToStore(value); }

		[ModelProperty(PropertyType.Collection)]
		public ModelPrimitivesList<KeyValuePair<string, int>> RawOptions { get => new(Options); set => Options = value.ToDictionary(s => s.Key, s => s.Value); }

		public IDictionary<string, int> Options { get => GetDataFromStore<IDictionary<string, int>>(); private set => SetDataToStore(value); }

		[ModelProperty(PropertyType.Primitive)]
		public string Title { get => GetDataFromStore<string>(); private set => SetDataToStore(value); }

		[ModelProperty(PropertyType.Model)]
		public MessageAliveHolderModel Message { get => GetDataFromStore<MessageAliveHolderModel>(); private set => SetDataToStore(value); }

		public override IServer Server => Message.Channel.Server;
	}
}
