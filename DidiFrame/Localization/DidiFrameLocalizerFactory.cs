using System.Reflection;

namespace DidiFrame.Localization
{
	/// <summary>
	/// DidiFrame localization part. Don't use this class anywhere
	/// </summary>
	public sealed class DidiFrameLocalizerFactory : IStringLocalizerFactory
	{
		private readonly static string[] moduleAssemblies = new[]
		{
			"DidiFrame.Clients.DSharp",
			"DidiFrame.Data.Json",
			"DidiFrame.Data.Mongo",
		};


		private readonly IDidiFrameLocalizationProvider[] instances;
		private readonly IStringLocalizerFactory? delegatedLocalizer;
		private readonly string overrideAssemblyName;


		/// <summary>
		/// DidiFrame localization part. Don't use this member anywhere
		/// </summary>
		public DidiFrameLocalizerFactory(string overrideAssemblyName)
		{
			var asmToAnalyze = Assembly.GetExecutingAssembly();
			var types = new List<Type>();
			types.AddRange(asmToAnalyze.GetTypes());

			foreach (var asm in moduleAssemblies)
			{
				var load = Assembly.Load(asm);
				types.AddRange(load.GetTypes());
			}

			instances = types.Where(s => s.IsAssignableTo(typeof(IDidiFrameLocalizationProvider))
				&& !(s.IsInterface || s.IsValueType || s.IsAbstract || s.IsGenericType) && isInternalOrPublic(s))
				.Select(s => Activator.CreateInstance(s) as IDidiFrameLocalizationProvider ?? throw new ImpossibleVariantException()).ToArray();


			static bool isInternalOrPublic(Type t)
			{
				return !t.IsNested
					&& !t.IsNestedPublic
					&& !t.IsNestedFamily
					&& !t.IsNestedPrivate
					&& !t.IsNestedAssembly
					&& !t.IsNestedFamORAssem
					&& !t.IsNestedFamANDAssem;
			}

			this.overrideAssemblyName = overrideAssemblyName;
		}

		/// <summary>
		/// DidiFrame localization part. Don't use this member anywhere
		/// </summary>
		public DidiFrameLocalizerFactory(IStringLocalizerFactory delegatedLocalizer, string asmName) : this(asmName)
		{
			this.delegatedLocalizer = delegatedLocalizer;
		}


		/// <summary>
		/// DidiFrame localization part. Don't use this member anywhere
		/// </summary>
		public IStringLocalizer Create(Type resourceSource)
		{
			var instance = instances.SingleOrDefault(s => s.TargetType == resourceSource);
			ILocaleDictionarySource source = instance is null ? new NoSource() : new Source(instance);
			var localizer = delegatedLocalizer?.Create(resourceSource.FullName ?? throw new ImpossibleVariantException(), overrideAssemblyName);
			return new DidiFrameLocalizer(source, localizer);
		}

		/// <summary>
		/// DidiFrame localization part. Don't use this member anywhere
		/// </summary>
		public IStringLocalizer Create(string baseName, string location)
		{
			if (delegatedLocalizer is null)
			{
				var noSource = new NoSource();
				return new DidiFrameLocalizer(noSource);
			}
			else
			{
				return delegatedLocalizer.Create(baseName, location);
			}
		}


		private sealed class Source : ILocaleDictionarySource
		{
			private readonly IDidiFrameLocalizationProvider provider;


			public Source(IDidiFrameLocalizationProvider provider)
			{
				this.provider = provider;
			}


			public LocaleDictionary GetLocaleDictionary()
			{
				return provider.GetDictionaryFor(Thread.CurrentThread.CurrentUICulture);
			}
		}

		private sealed class NoSource : ILocaleDictionarySource
		{
			private readonly LocaleDictionary dic;


			public NoSource()
			{
				dic = new LocaleDictionary(new Dictionary<string, string>());
			}


			public LocaleDictionary GetLocaleDictionary()
			{
				return dic;
			}
		}
	}
}
