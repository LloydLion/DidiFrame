using System.Reflection;

namespace DidiFrame.Localization
{
	public class DidiFrameLocalizerFactory : IStringLocalizerFactory
	{
		private readonly IDidiFrameLocalizationProvider[] instances;
		private readonly IStringLocalizerFactory? delegatedLocalizer;
		private readonly string asmName;


		public DidiFrameLocalizerFactory(string asmName)
		{
			var asmToAnalyze = Assembly.GetExecutingAssembly();
			instances = asmToAnalyze.GetTypes().Where(s => s.IsAssignableTo(typeof(IDidiFrameLocalizationProvider))
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

			this.asmName = asmName;
		}

		public DidiFrameLocalizerFactory(IStringLocalizerFactory delegatedLocalizer, string asmName) : this(asmName)
		{
			this.delegatedLocalizer = delegatedLocalizer;
		}


		public IStringLocalizer Create(Type resourceSource)
		{
			var instance = instances.SingleOrDefault(s => s.TargetType == resourceSource);
			ILocaleDictionarySource source = instance is null ? new NoSource() : new Source(instance);
			var localizer = delegatedLocalizer?.Create(resourceSource.FullName ?? throw new ImpossibleVariantException(), asmName);
			return new DidiFrameLocalizer(source, localizer);
		}

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


		private class Source : ILocaleDictionarySource
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

		private class NoSource : ILocaleDictionarySource
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
