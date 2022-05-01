using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidiFrame.UserCommands.Pipeline.Utils
{
	public class DelegateMiddleware<TInput, TOutput> : AbstractUserCommandPipelineMiddleware<TInput, TOutput> where TInput : notnull where TOutput : notnull
	{
		private readonly Func<TInput, TOutput?> selector;


		public DelegateMiddleware(Func<TInput, TOutput?> selector)
		{
			this.selector = selector;
		}


		public override TOutput? Process(TInput input, UserCommandPipelineContext pipelineContext)
		{
			var res = selector(input);
			if (res is null)
				pipelineContext.DropPipeline();
			return res;
		}
	}
}
