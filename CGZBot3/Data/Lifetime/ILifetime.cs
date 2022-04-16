using CGZBot3.Utils;

namespace CGZBot3.Data.Lifetime
{
	public interface ILifetime<TBase> where TBase : class, ILifetimeBase
	{
		public void Run(ILifetimeStateUpdater<TBase> updater);

		public TBase GetBaseClone();
	}
}