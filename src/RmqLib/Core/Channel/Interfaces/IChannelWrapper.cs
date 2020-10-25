using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

namespace RmqLib.Core {
	internal interface IChannelWrapper {
		Task<PublishStatus> BasicPublish(PublishItem deliveryInfo);
		void Close();
		void UnlockChannel();
		void LockChannel();
		IModel GetTrueChannel();
		Task BasicAck(ulong deliveryTag, bool multiple);
		Task BasicReject(ulong deliveryTag, bool requeue);
		Task QueueBind(string queue, string exchange, string routingKey);
		Task BasicConsume(IBasicConsumer consumer, string queue, bool autoAck);
		Task<IBasicProperties> CreateBasicProperties();
		Task BasicPublish(string exchange, string routingKey, IBasicProperties basicProperties, byte[] body = null);
	}
}