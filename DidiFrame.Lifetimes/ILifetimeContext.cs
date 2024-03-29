﻿using DidiFrame.Utils;

namespace DidiFrame.Lifetimes
{
	/// <summary>
	/// Represents eviroment where lifetime runs
	/// </summary>
	/// <typeparam name="TBase">Type of lifetime base</typeparam>
	public interface ILifetimeContext<TBase> where TBase : class, ILifetimeBase
	{
		/// <summary>
		/// Is lifetime is newly created else is restored from
		/// </summary>
		public bool IsNewlyCreated { get; }

		/// <summary>
		/// Provides controller to access to base object
		/// </summary>
		/// <returns></returns>
		public IObjectController<TBase> AccessBase();

		/// <summary>
		/// Finalizes lifetime and clears all resources and records
		/// </summary>
		public void FinalizeLifetime();

		/// <summary>
		/// Provides current lifetime sync context
		/// </summary>
		/// <returns>Current lifetime context</returns>
		public LifetimeSynchronizationContext GetSynchronizationContext();
	}
}
