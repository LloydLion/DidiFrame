using System.Reflection;

namespace DidiFrame.Dependencies
{
	/// <summary>
	/// Extensions methods for IServiceProvider to dynamically create objects with dependencies
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Creates instance of given type and resolves all some constructor parameter as dependencies
		/// </summary>
		/// <typeparam name="TObject">Target object type</typeparam>
		/// <param name="services">Services to provide dependencies</param>
		/// <returns>New resolved object</returns>
		/// <exception cref="Exception">If cannot resolve target type</exception>
		public static TObject ResolveDependencyObject<TObject>(this IServiceProvider services) => (TObject)services.ResolveDependencyObject(typeof(TObject));

		/// <summary>
		/// Creates instance of given type and resolves all some constructor parameter as dependencies
		/// </summary>
		/// <param name="services">Services to provide dependencies</param>
		/// <param name="type">Target object type</param>
		/// <returns>New resolved object</returns>
		/// <exception cref="Exception">If cannot resolve target type</exception>
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


		/// <summary>
		/// Creates instance of given type and resolves all parameters that marked with DependencyAttribute of given constructor as dependencies
		/// and all other parameters fill with given preParemters
		/// </summary>
		/// <typeparam name="TObject">Type of resolving object</typeparam>
		/// <param name="services">Services to provide dependencies</param>
		/// <param name="constructor">Target constructor to analyze</param>
		/// <param name="preParameters">Preparameters for constructor</param>
		/// <returns>New resolved object</returns>
		/// <exception cref="Exception">If cannot resolve object using given constructor</exception>
		public static TObject ResolveObjectWithDependencies<TObject>(this IServiceProvider services, ConstructorInfo constructor, IReadOnlyList<object> preParameters) =>
			(TObject)services.ResolveObjectWithDependencies(constructor, preParameters);

		/// <summary>
		/// Creates instance of given type and resolves all parameters that marked with DependencyAttribute of found constructor as dependencies
		/// and finds target constructor by transmited preParameters
		/// </summary>
		/// <typeparam name="TObject">Target type to analyze</typeparam>
		/// <param name="services">Services to provide dependencies</param>
		/// <param name="preParameters">Preparameters to find constructor</param>
		/// <returns>New resolved object</returns>
		/// <exception cref="Exception">If cannot resolve object using given constructor</exception>
		public static TObject ResolveObjectWithDependencies<TObject>(this IServiceProvider services, IReadOnlyList<object> preParameters) =>
			(TObject)services.ResolveObjectWithDependencies(typeof(TObject), preParameters);

		/// <summary>
		/// Creates instance of given type and resolves all parameters that marked with DependencyAttribute of given constructor as dependencies
		/// and all other parameters fill with given preParemters
		/// </summary>
		/// <param name="services">Services to provide dependencies</param>
		/// <param name="constructor">Target constructor to analyze</param>
		/// <param name="preParameters">Preparameters for constructor</param>
		/// <returns>New resolved object</returns>
		/// <exception cref="Exception">If cannot resolve object using given constructor</exception>
		public static object ResolveObjectWithDependencies(this IServiceProvider services, ConstructorInfo constructor, IReadOnlyList<object> preParameters)
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

			if (failed) throw new Exception($"Enable to resole {constructor.DeclaringType?.FullName} type using given constructor");
			else return constructor.Invoke(parametersValues);
		}

		/// <summary>
		/// Creates instance of given type and resolves all parameters that marked with DependencyAttribute of found constructor as dependencies
		/// and finds target constructor by transmited preParameters
		/// </summary>
		/// <param name="services">Services to provide dependencies</param>
		/// <param name="type">Target type to analyze</param>
		/// <param name="preParameters">Preparameters to find constructor</param>
		/// <returns>New resolved object</returns>
		/// <exception cref="Exception">If cannot resolve object using given constructor</exception>
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

					return services.ResolveObjectWithDependencies(ctor, preParameters);
				}
			}

			throw new Exception($"Enable to find constructor in {type.FullName} that given parameters can resolve");
		}
	}
}
