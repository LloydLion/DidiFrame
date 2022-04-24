namespace DidiFrame.Data.Lifetime
{
	public interface IStateBasedLifetimeBase<TState> : ILifetimeBase, ICloneable, IEquatable<IStateBasedLifetimeBase<TState>> where TState : struct
	{
		public TState State { get; set; }
	}
}
