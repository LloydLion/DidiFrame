namespace DidiFrame.Utils.StateMachine
{
	public static class Extensions
	{
		public static IStateMachine<TState> FromProfile<TState>(this IStateMachineBuilderFactory<TState> factory, string logCategory,
			StateMachineProfile<TState> profile) where TState : struct
		{
			var builder = factory.Create(logCategory);
			builder.AddProfile(profile);
			return builder.Build();
		}

		public static void AddProfile<TState>(this IStateMachineBuilder<TState> builder, StateMachineProfile<TState> profile) where TState : struct
		{
			foreach (var worker in profile.StateWorkers) builder.AddStateTransitWorker(worker);
			foreach (var handler in profile.StateChangedHandlers) builder.AddStateChangedHandler(handler);
		}
	}
}
