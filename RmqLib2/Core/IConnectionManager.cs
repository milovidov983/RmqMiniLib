using System.Threading.Tasks;

namespace RmqLib2 {
	internal interface IConnectionManager {
		void Reconnect();
		IPublisherFactory CreatePublisherFactory { get; }
	}
}
