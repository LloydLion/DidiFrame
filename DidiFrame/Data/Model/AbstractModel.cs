using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DidiFrame.Data.Model
{
	public abstract class AbstractModel : IDataModel
	{
		private readonly Dictionary<string, object> abstractDataStore = new();
		private readonly PropertyInfo[] properties;
		private int writingAbility = 0;

		public abstract IServer Server { get; }

		public Guid Id { get; }


		public event PropertyChangedEventHandler? PropertyChanged;

		public event PropertyChangingEventHandler? PropertyChanging;


		protected AbstractModel()
		{
			Id = Guid.NewGuid();

			var typeToAnalize = GetType();
			properties = typeToAnalize.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).Where(s => s.CanWrite || s.GetCustomAttribute<ModelPropertyAttribute>() is not null).ToArray();
		}


		protected AbstractModel(ISerializationModel model)
		{
			var guidString = model.ReadPrimitive<string>("_Id");
			if (guidString is not null) Id = Guid.Parse(guidString);
			else Id = Guid.NewGuid();

			var typeToAnalize = GetType();
			properties = typeToAnalize.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).Where(s => s.CanWrite || s.GetCustomAttribute<ModelPropertyAttribute>() is not null).ToArray();

			foreach (var property in properties)
			{
				var attr = property.GetCustomAttribute<ModelPropertyAttribute>() ?? throw new ImpossibleVariantException();
				var setMethod = property.GetSetMethod(nonPublic: true) ?? throw new ImpossibleVariantException();

				object? data = attr.Type switch
				{
					PropertyType.Primitive => model.ReadPrimitive(property.PropertyType, property.Name),
					PropertyType.Model => model.ReadModel(property.PropertyType, property.Name),
					PropertyType.Collection => model.ReadCollection(property.PropertyType, property.Name),
					_ => throw new NotSupportedException()
				};

				setMethod.Invoke(this, new[] { data });
			}
		}


		public IDisposable PreventWritings()
		{
			var handlers = new List<IDisposable>();

			foreach (var property in properties)
			{
				var attr = property.GetCustomAttribute<ModelPropertyAttribute>() ?? throw new ImpossibleVariantException();
				if (attr.Type != PropertyType.Model) continue;

				var getMethod = property.GetGetMethod(nonPublic: true) ?? throw new ImpossibleVariantException();

				var data = (IDataModel?)getMethod.Invoke(this, null);

				if (data is not null) handlers.Add(data.PreventWritings());
			}

			return new WritingsPreventer(this, handlers);
		}

		public void SerializeTo(ISerializationModelBuilder builder)
		{
			builder.WritePrimitive("_Id", Id.ToString());

			foreach (var property in properties)
			{
				var attr = property.GetCustomAttribute<ModelPropertyAttribute>() ?? throw new ImpossibleVariantException();
				var getMethod = property.GetGetMethod(nonPublic: true) ?? throw new ImpossibleVariantException();

				object? data = getMethod.Invoke(this, null);

				switch (attr.Type)
				{
					case PropertyType.Collection:
						builder.WriteCollection(property.Name, (IDataCollection?)data);
						break;
					case PropertyType.Model:
						builder.WriteModel(property.Name, (IDataModel?)data);
						break;
					case PropertyType.Primitive:
						builder.WritePrimitive(property.Name, data);
						break;
					default:
						throw new NotSupportedException();
				}
			}

			AdditionalSerializeTo(builder);
		}

		public bool Equals(IDataModel? other)
		{
			return other is not null && other.GetType() == GetType() && other.Id == Id;
		}

		public override bool Equals(object? obj) => Equals(obj as IDataModel);

		public override int GetHashCode() => Id.GetHashCode();

		protected virtual void AdditionalSerializeTo(ISerializationModelBuilder builder) { }

		protected T GetDataFromStore<T>([CallerMemberName] string propertyName = "#caller member name#") => (T)abstractDataStore[propertyName];

		protected void SetDataToStore(object data, [CallerMemberName] string propertyName = "#caller member name#")
		{
			if (writingAbility != 0) throw new ModelBlockedToWriteException(propertyName);
			PropertyChanging?.Invoke(this, new(propertyName));
			abstractDataStore[propertyName] = data;
			PropertyChanged?.Invoke(this, new(propertyName));
		}


		[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
		protected class ModelPropertyAttribute : Attribute
		{
			public ModelPropertyAttribute(PropertyType type)
			{
				Type = type;
			}


			public PropertyType Type { get; }
		}

		private sealed class WritingsPreventer : IDisposable
		{
			private readonly AbstractModel owner;
			private readonly IEnumerable<IDisposable> disposables;


			public WritingsPreventer(AbstractModel owner, IEnumerable<IDisposable> disposables)
			{
				owner.writingAbility += 1;
				this.owner = owner;
				this.disposables = disposables;
			}


			public void Dispose()
			{
				owner.writingAbility += 1;

				foreach (var item in disposables) item.Dispose();
			}
		}


		protected enum PropertyType
		{
			Collection,
			Model,
			Primitive
		}
	}
}
