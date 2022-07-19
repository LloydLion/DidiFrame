using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidiFrame.Data
{
	public class ServerStateHolder<TModel> where TModel : class
	{
		private readonly Func<TModel> modelSource;
		private readonly Action<TModel> finalizer;


		public ServerStateHolder(Func<TModel> modelSource, Action<TModel> finalizer)
		{
			this.modelSource = modelSource;
			this.finalizer = finalizer;
		}

		public ServerStateHolder(Func<IServer, TModel> modelSource, Action<IServer, TModel> finalizer, IServer parameter)
		{
			var recaster = new FuncRecaster(modelSource, finalizer, parameter);
			this.modelSource = recaster.GetModel;
			this.finalizer = recaster.FinalizerModel;
		}


		public IDisposable Open(out TModel model)
		{
			var savedModel = modelSource();
			model = savedModel;
			return new Disposable(finalizer, savedModel);
		}


		private class Disposable : IDisposable
		{
			private readonly Action<TModel> finalizer;
			private readonly TModel model;


			public Disposable(Action<TModel> finalizer, TModel model)
			{
				this.finalizer = finalizer;
				this.model = model;
			}


			public void Dispose()
			{
				finalizer(model);
			}
		}

		private class FuncRecaster
		{
			private readonly Func<IServer, TModel> modelSource;
			private readonly Action<IServer, TModel> finalizer;
			private readonly IServer parameter;


			public FuncRecaster(Func<IServer, TModel> modelSource, Action<IServer, TModel> finalizer, IServer parameter)
			{
				this.modelSource = modelSource;
				this.finalizer = finalizer;
				this.parameter = parameter;
			}


			public TModel GetModel() => modelSource(parameter);

			public void FinalizerModel(TModel model) => finalizer(parameter, model);
		}
	}
}
