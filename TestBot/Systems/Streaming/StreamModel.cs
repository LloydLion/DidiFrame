using DidiFrame.Data.AutoKeys;
using DidiFrame.Lifetimes;
using DidiFrame.Data.Model;
using DidiFrame.Utils;

namespace TestBot.Systems.Streaming
{
	[DataKey(StatesKeys.StreamingSystem)]
	public class StreamModel : AbstractModel, IStateBasedLifetimeBase<StreamState>
	{
		public StreamModel(string name, IMember member, DateTime planedStartTime, string rawPlace, ITextChannel channel)
		{
			Name = name;
			Owner = member;
			PlanedStartTime = planedStartTime;
			Place = rawPlace;
			ReportMessage = new(channel);
		}

#nullable disable
		public StreamModel(ISerializationModel model) : base(model) { }
#nullable restore


		[ModelProperty(PropertyType.Primitive)]
		public string Name { get => GetDataFromStore<string>(); set => SetDataToStore(value); }

		[ModelProperty(PropertyType.Primitive)]
		public IMember Owner { get => GetDataFromStore<IMember>(); private set => SetDataToStore(value); }

		[ModelProperty(PropertyType.Primitive)]
		public DateTime PlanedStartTime { get => GetDataFromStore<DateTime>(); set => SetDataToStore(value); }

		[ModelProperty(PropertyType.Primitive)]
		public string Place { get => GetDataFromStore<string>(); set => SetDataToStore(value); }

		[ModelProperty(PropertyType.Model)]
		public MessageAliveHolderModel ReportMessage { get => GetDataFromStore<MessageAliveHolderModel>(); private set => SetDataToStore(value); }

		[ModelProperty(PropertyType.Primitive)]
		public StreamState State { get => GetDataFromStore<StreamState>(); set => SetDataToStore(value); }

		public override IServer Server => Owner.Server;
	}
}
