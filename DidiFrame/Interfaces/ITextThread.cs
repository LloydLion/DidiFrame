using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidiFrame.Interfaces
{
	public interface ITextThread : ITextChannelBase
	{
		public ITextChannel Parent { get; }
	}
}
