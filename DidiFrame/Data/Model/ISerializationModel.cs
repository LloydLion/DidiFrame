namespace DidiFrame.Data.Model
{
	public interface ISerializationModel
	{
		public object? ReadPrimitive(Type typeToRead, string propertyName);

		public IDataModel? ReadModel(Type typeToRead, string propertyName);

		public IDataCollection? ReadCollection(Type typeToRead, string propertyName);
	}
}
