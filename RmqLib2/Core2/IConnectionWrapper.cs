using RabbitMQ.Client;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal interface IConnectionWrapper {
		IModel CreateChannel();
		void StartConnection();
	}
}
