﻿using DidiFrame.Data.AutoKeys;
using DidiFrame.Lifetimes;
using DidiFrame.Data.Model;
using DidiFrame.Utils;

namespace TestBot.Systems.Streaming
{
	[DataKey(StatesKeys.StreamingSystem)]
	public class StreamModel : IStateBasedLifetimeBase<StreamState>
	{
		[SerializationConstructor]
		public StreamModel(string name, IMember member, DateTime planedStartTime, string rawPlace, MessageAliveHolderModel reportMessage, Guid id)
		{
			Name = name;
			Owner = member;
			PlanedStartTime = planedStartTime;
			Place = rawPlace;
			ReportMessage = reportMessage;
			Id = id;
		}

		public StreamModel(string name, IMember member, DateTime planedStartTime, string rawPlace, ITextChannel channel)
			: this(name, member, planedStartTime, rawPlace, new MessageAliveHolderModel(channel), Guid.NewGuid()) { }


		[ConstructorAssignableProperty(0, "name")]
		public string Name { get; set; }

		[ConstructorAssignableProperty(1, "member")]
		public IMember Owner { get; }

		[ConstructorAssignableProperty(2, "planedStartTime")]
		public DateTime PlanedStartTime { get; set; }

		[ConstructorAssignableProperty(3, "rawPlace")]
		public string Place { get; set; }

		[ConstructorAssignableProperty(4, "reportMessage")]
		public MessageAliveHolderModel ReportMessage { get; }

		[ConstructorAssignableProperty(5, "id")]
		public Guid Id { get; }

		public StreamState State { get; set; }

		public IServer Server => Owner.Server;


		public bool Equals(IDataModel? other) =>
			other is StreamModel model && model.Id == Id;

		public override bool Equals(object? obj) => Equals(obj as IDataModel);

		public override int GetHashCode() => Id.GetHashCode();
	}
}
