namespace CGZBot3.Utils.StateMachine
{
	internal class StateMachineBuilderFactory<TState> : IStateMachineBuilderFactory<TState> where TState : struct
	{
		private readonly ILoggerFactory factory;


		public StateMachineBuilderFactory(ILoggerFactory factory)
		{
			this.factory = factory;
		}


		public IStateMachineBuilder<TState> Create(string logCategory)
		{
			return new StateMachineBuilder<TState>(factory.CreateLogger(logCategory));
		}
	}
}
