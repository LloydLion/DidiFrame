using System.ComponentModel;

namespace DidiFrame.Data.Model
{
	/*
	 * Implementator must has public constuctor with single paramter of ISerializationModel type
	 */
	public interface IDataModel : IDataEntity, INotifyPropertyChanged, INotifyPropertyChanging, IEquatable<IDataModel>
	{
		public IServer Server { get; }

		public Guid Id { get; }


		public void SerializeTo(ISerializationModelBuilder builder);
	}
}
