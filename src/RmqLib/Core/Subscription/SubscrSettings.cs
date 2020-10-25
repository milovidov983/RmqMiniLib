using RabbitMQ.Client;

namespace RmqLib {

	internal class SubscrSettings {
		public IModel Model { get; set; }
		public int PrefechCount { get; set; }
		public IRabbitCommand Command { get; set; }
		public string Topic { get; set; }
	}
	
}
