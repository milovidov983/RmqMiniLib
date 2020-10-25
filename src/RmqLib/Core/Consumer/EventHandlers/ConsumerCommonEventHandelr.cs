using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace RmqLib.Core {
	internal class ConsumerCommonEventHandelr : IConsumerCommonEventHandelr {
		private readonly IRmqLogger logger;

		public ConsumerCommonEventHandelr(IRmqLogger logger) {
			this.logger = logger;
		}

		public Task ConsumerCancelledHandler(object sender, ConsumerEventArgs @event) {
			logger.Warn("ConsumerEvent: consumer cancelled");
			return Task.CompletedTask;
		}

		public Task ShutdownHandler(object sender, ShutdownEventArgs @event) {
			logger.Warn($"ConsumerEvent: consumer shutdown. ReplyText: {@event.ReplyText}");
			return Task.CompletedTask;
		}

		public Task UnregisteredHandler(object sender, ConsumerEventArgs @event) {
			logger.Warn("ConsumerEvent: consumer unregistered");
			return Task.CompletedTask;
		}
	}
}