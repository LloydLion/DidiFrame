namespace DidiFrame.Utils
{
	public class SelectObjectContoller<TBase, TTarget> : IObjectController<TTarget> where TBase : class where TTarget : class
	{
		private readonly IObjectController<TBase> baseController;
		private readonly Func<TBase, TTarget> selector;


		public SelectObjectContoller(IObjectController<TBase> baseController, Func<TBase, TTarget> selector)
		{
			this.baseController = baseController;
			this.selector = selector;
		}


		public ObjectHolder<TTarget> Open()
		{
			var oh = baseController.Open();
			return new ObjectHolder<TTarget>(selector(oh.Object), _ =>
			{
				oh.Dispose();
			});
		}
	}
}
