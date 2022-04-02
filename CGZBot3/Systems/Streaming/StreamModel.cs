using CGZBot3.Data.Lifetime;
using CGZBot3.Data.Model;
using CGZBot3.Utils;

namespace CGZBot3.Systems.Streaming
{
	public class StreamModel : IStateBasedLifetimeBase<StreamState>
	{
		private static int nextId = 0;


		public StreamModel(string name, IMember member, DateTime planedStartTime, string rawPlace, MessageAliveHolder.Model reportMessage, int id)
		{
			Name = name;
			Owner = member;
			PlanedStartTime = planedStartTime;
			Place = rawPlace;
			ReportMessage = reportMessage;
			Id = id;
			nextId = Math.Max(id, nextId);
		}

		public StreamModel(string name, IMember member, DateTime planedStartTime, string rawPlace, ITextChannel channel)
			: this(name, member, planedStartTime, rawPlace, new MessageAliveHolder.Model(channel), ++nextId) { }


		[ConstructorAssignableProperty(0, "name")]
		public string Name { get; set; }

		[ConstructorAssignableProperty(1, "member")]
		public IMember Owner { get; }

		[ConstructorAssignableProperty(2, "planedStartTime")]
		public DateTime PlanedStartTime { get; set; }

		[ConstructorAssignableProperty(3, "rawPlace")]
		public string Place { get; set; }

		[ConstructorAssignableProperty(4, "reportMessage")]
		public MessageAliveHolder.Model ReportMessage { get; }

		[ConstructorAssignableProperty(5, "id")]
		public int Id { get; }

		public StreamState State { get; set; }

		public IServer Server => Owner.Server;


		public object Clone() => new StreamModel(Name, Owner, PlanedStartTime, Place, ReportMessage, Id) { State = State };

		public bool Equals(IStateBasedLifetimeBase<StreamState>? other) => other is StreamModel stream && stream.Id == Id;

		public override bool Equals(object? obj) => obj is StreamModel stream && stream.Id == Id;

		public override int GetHashCode() => Id.GetHashCode();
	}
}
