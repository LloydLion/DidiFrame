namespace CGZBot3.Data.Lifetime
{
	public class LifetimeHolder<TLifetime, TBase> where TLifetime : ILifetime<TBase> where TBase : class, ILifetimeBase
	{
		private readonly LifetimeHolder<TLifetime, TBase>.EventFire fire;


		private LifetimeHolder(TLifetime lifetime, EventFire fire)
		{
			Lifetime = lifetime;
			this.fire = fire;
		}


		public TLifetime Lifetime { get; }


		public event Action? StateUpdated { add { fire.StateUpdated += value; } remove { fire.StateUpdated -= value; } }

		public event Action? LifetimeFinalized { add { fire.LifetimeFinalized += value; } remove { fire.LifetimeFinalized -= value; } }


		public static LifetimeHolder<TLifetime, TBase> Create(TLifetime lifetime, out EventFire fire)
		{
			fire = new EventFire();
			return new LifetimeHolder<TLifetime, TBase>(lifetime, fire);
		}


		public class EventFire
		{
			public event Action? StateUpdated;

			public event Action? LifetimeFinalized;


			public void InvokeStateUpdated() { StateUpdated?.Invoke(); }

			public void InvokeLifetimeFinalized() { LifetimeFinalized?.Invoke(); }
		}
	}
}
