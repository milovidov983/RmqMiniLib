using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib {
	/// <summary>
	/// Channel
	/// /// </summary>
	public class Channel: IChannel {
		/// <summary>
		/// TODO comment
		/// </summary>
		private readonly IModel channel;
		/// <summary>
		/// TODO comment
		/// </summary>
		private readonly string exchange;


		/// <summary>
		/// TODO comment
		/// </summary>
		public Channel(IModel channel, string exchange) {
			this.channel = channel;
			this.exchange = exchange;
		}

		/// <summary>
		/// Send RPC request
		/// </summary>
		/// <param name="topic">topic</param>
		/// <param name="payload">payload</param>
		/// <returns>correlationId</returns>
		public string SendRpc(string topic, byte[] payload) {
			var props = channel.CreateBasicProperties();
			var correlationId = Guid.NewGuid().ToString();

			props.CorrelationId = correlationId;
			props.ReplyTo = ServiceConstants.REPLY_QUEUE_NAME;

			channel.BasicPublish(
				exchange: exchange,
				routingKey: topic,
				basicProperties: props,
				body: payload);

			return correlationId;

		}


		public void BindReplyQueue(Ittt responseHandler) {
			var consumer = new AsyncEventingBasicConsumer(channel);
			channel.BasicConsume(
				consumer: consumer,
				queue: ServiceConstants.REPLY_QUEUE_NAME,
				autoAck: true);
			consumer.Received += responseHandler.ReceiveReply;
		}

		public async Task ReceiveReply(object model, BasicDeliverEventArgs ea) {
			throw new NotImplementedException();
		}
	}

	public interface Ittt {
		Task ReceiveReply(object model, BasicDeliverEventArgs ea);
	}
}