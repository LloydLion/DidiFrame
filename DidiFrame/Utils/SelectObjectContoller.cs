namespace DidiFrame.Utils
{
	/// <summary>
	/// Object controller that selects some property from object and delegates control to other controller
	/// </summary>
	/// <typeparam name="TBase">Type of base controller</typeparam>
	/// <typeparam name="TTarget">Target type</typeparam>
	public class SelectObjectContoller<TBase, TTarget> : IObjectController<TTarget> where TBase : class where TTarget : class
	{
		private readonly IObjectController<TBase> baseController;
		private readonly Func<TBase, TTarget> selector;


		/// <summary>
		/// Creates new instance of DidiFrame.Utils.SelectObjectContoller`2
		/// </summary>
		/// <param name="baseController">Base controller to get object</param>
		/// <param name="selector">Property selector</param>
		public SelectObjectContoller(IObjectController<TBase> baseController, Func<TBase, TTarget> selector)
		{
			this.baseController = baseController;
			this.selector = selector;
		}


		/// <inheritdoc/>
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
