﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Interfaces
{
	internal interface IServerEntity : IEquatable<IServerEntity>
	{
		public IServer Server { get; }
	}
}