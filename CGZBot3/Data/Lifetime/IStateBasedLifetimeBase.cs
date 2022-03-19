namespace CGZBot3.Data.Lifetime
{
	public interface IStateBasedLifetimeBase<TState> : ILifetimeBase, ICloneable where TState : struct
	{
		public TState State { get; set; }
	}
}
