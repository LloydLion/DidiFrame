using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Exceptions
{
	internal class InvalidSettingsException : Exception
	{
		public InvalidSettingsException(string? message, Exception? innerException) : base(message, innerException) { }
	}
}
