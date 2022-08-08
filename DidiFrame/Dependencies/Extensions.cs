using System.Reflection;

namespace DidiFrame.Dependencies
{
	public static class Extensions
	{
		public static TObject ResolveDependencyObject<TObject>(this IServiceProvider services) => (TObject)services.ResolveDependencyObject(typeof(TObject));

		public static object ResolveDependencyObject(this IServiceProvider services, Type type)
		{
			var ctors = type.GetConstructors();

			foreach (var ctor in ctors)
			{
				var failed = false;
				var parameters = ctor.GetParameters();
				var parametersValues = new object?[parameters.Length];

				for (int i = 0; i < parameters.Length; i++)
				{
					var para = parameters[i];
					var service = services.GetService(para.ParameterType);

					if (para.IsOptional == false && service is null)
					{
						failed = true;
						break;
					}
					else parametersValues[i] = service;
				}

				if (failed) continue;
				return ctor.Invoke(parametersValues);
			}

			throw new Exception($"Enable to resole {type.FullName} type");
		}

		public static TObject ResolveObjectWithDependencies<TObject>(this IServiceProvider services, ConstructorInfo constructor, IReadOnlyList<object> preParameters) =>
			(TObject)services.ResolveObjectWithDependencies(typeof(TObject), constructor, preParameters);

		public static TObject ResolveObjectWithDependencies<TObject>(this IServiceProvider services, IReadOnlyList<object> preParameters) =>
			(TObject)services.ResolveObjectWithDependencies(typeof(TObject), preParameters);

		public static object ResolveObjectWithDependencies(this IServiceProvider services, Type type, ConstructorInfo constructor, IReadOnlyList<object> preParameters)
		{
			var failed = false;
			var ctorParameters = constructor.GetParameters();
			var parametersValues = new object?[ctorParameters.Length];

			var preParasEnumerator = preParameters.GetEnumerator();

			for (int i = 0; i < ctorParameters.Length; i++)
			{
				var para = ctorParameters[i];

				if (para.GetCustomAttribute<DependencyAttribute>() is not null)
				{
					var service = services.GetService(para.ParameterType);

					if (para.IsOptional == false && service is null)
					{
						failed = true;
						break;
					}
					else parametersValues[i] = service;
				}
				else
				{
					preParasEnumerator.MoveNext();
					parametersValues[i] = preParasEnumerator.Current;
				}
			}

			if (failed) throw new Exception($"Enable to resole {type.FullName} type using given constructor");
			else return constructor.Invoke(parametersValues);
		}
		
		public static object ResolveObjectWithDependencies(this IServiceProvider services, Type type, IReadOnlyList<object> preParameters)
		{
			var ctors = type.GetConstructors();

			foreach (var ctor in ctors)
			{
				var parameters = ctor.GetParameters().Where(s => s.GetCustomAttribute<DependencyAttribute>() is null).ToArray();

				if (parameters.Length != preParameters.Count) continue;

				bool isFailed = false;

				for (int i = 0; i < preParameters.Count; i++)
				{
					var preParameterValue = preParameters[i];
					var parameter = parameters[i];

					if (parameter.ParameterType.IsInstanceOfType(preParameterValue) == false)
					{
						isFailed = true;
						break;
					}
				}

				if (isFailed) continue;
				else
				{
					//Match case

					return services.ResolveObjectWithDependencies(type, ctor, preParameters);
				}
			}

			throw new Exception($"Enable to find constructor in {type.FullName} that given parameters can resolve");
		}
	}
}
