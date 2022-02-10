using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.UserCommands.Loader
{
	internal class ReflectionUserCommandsLoader : IUserCommandsLoader
	{
		private readonly Options options;

		
		public ReflectionUserCommandsLoader(IOptions<Options> options)
		{
			this.options = options.Value;
		}


		public void LoadTo(IUserCommandsRepository rp)
		{
			var types = options.Types.Select(s => Type.GetType(s, true, true)).ToArray();

			foreach (var type in types)
			{
				var instance = Activator.CreateInstance(type ?? throw new Exception()) ?? throw new Exception();

				var methods = type.GetMethods().Where(s => s.GetCustomAttributes(typeof(CommandAttribute), false).Length == 1);

				foreach (var method in methods)
				{
					var commandName = ((CommandAttribute)method.GetCustomAttributes(typeof(CommandAttribute), false)[0]).Name;

					var @params = method.GetParameters();
					var args = new UserCommandInfo.Argument[@params.Length - 1];
					for (int i = 1; i < @params.Length; i++)
					{
						var ptype = @params[i].ParameterType;
						var switchOn = ptype.IsArray && i == @params.Length - 1 ? ptype.GetElementType() ?? throw new Exception() : ptype;

						var atype = switchOn.FullName switch
						{
							"System.Int32" => UserCommandInfo.Argument.Type.Integer,
							"System.Double" => UserCommandInfo.Argument.Type.Double,
							"System.String" => UserCommandInfo.Argument.Type.String,
							"System.TimeSpan" => UserCommandInfo.Argument.Type.TimeSpan,
							"CGZBot3.Interfaces.IMember" => UserCommandInfo.Argument.Type.Member,
							"CGZBot3.Interfaces.IRole" => UserCommandInfo.Argument.Type.Role,
							"System.Object" => UserCommandInfo.Argument.Type.Mentionable,
							_ => throw new Exception($"Can't load comamnds, some parameter has invalid type." +
								$" In {type} in {method.Name} at {i} position parameter"),
						};

						args[i - 1] = new UserCommandInfo.Argument(ptype.IsArray && i == @params.Length - 1, atype, @params[i].Name ?? "no_name");
					}

					rp.AddCommand(new UserCommandInfo(commandName, new Handler(method, instance).HandleAsync, args));
				}
			}
		}


		public class Options
		{
			public string[] Types { get; init; } = Array.Empty<string>();
		}

		private readonly struct Handler
		{
			private readonly MethodInfo method;
			private readonly object obj;


			public Handler(MethodInfo method, object obj)
			{
				this.method = method;
				this.obj = obj;
			}


			public Task<UserCommandResult> HandleAsync(UserCommandContext ctx)
			{
				return (Task<UserCommandResult>)(method.Invoke(obj, ctx.Arguments.Values.Prepend(ctx).ToArray()) ??
					throw new NullReferenceException("Handler method's return was null"));
			}
		}
	}
}
