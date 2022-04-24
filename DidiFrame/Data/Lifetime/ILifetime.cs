using DidiFrame.Utils;

namespace DidiFrame.Data.Lifetime
{
	public interface ILifetime<TBase> where TBase : class, ILifetimeBase
	{
		public void Run(ILifetimeStateUpdater<TBase> updater);

		public TBase GetBaseClone();
	}
}