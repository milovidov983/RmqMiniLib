using RabbitMQ.Client;

namespace RmqLib2 {
	internal interface IConnectionWrapper {
		IModel CreateChannel();
		void StartConnection();
		bool IsOpen { get; }
		void AddConnectionShutdownHandler(IConnectionManager connectionManager);
	}
}
