﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Utils.StateMachine
{
	public interface IStateMachineBuilderFactory<TState> where TState : struct
	{
		public IStateMachineBuilder<TState> Create(string logCategory);
	}
}
