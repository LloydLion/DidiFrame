using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Data
{
	internal interface IModelFactory<out TModel>
	{
		public TModel CreateDefault();
	}
}
