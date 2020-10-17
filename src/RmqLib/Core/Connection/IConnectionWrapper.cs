using RabbitMQ.Client;

namespace RmqLib {
	internal interface IConnectionWrapper {
		IModel CreateChannel();
		bool IsOpen { get; }
		void AddConnectionShutdownHandler(IConnectionManager connectionManager);
	}
}
