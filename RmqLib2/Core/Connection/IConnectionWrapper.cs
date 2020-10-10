using RabbitMQ.Client;

namespace RmqLib2 {
	internal interface IConnectionWrapper {
		IModel CreateChannel();
		bool IsOpen { get; }
		void AddConnectionShutdownHandler(IConnectionManager connectionManager);
	}
}
