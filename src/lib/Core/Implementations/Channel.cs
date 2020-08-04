using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

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
	}
}