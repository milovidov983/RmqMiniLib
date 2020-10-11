using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib.Core {
	/// <summary>
	/// Channel
	/// /// </summary>
	internal class Channel: IChannel {
		/// <summary>
		/// TODO comment
		/// </summary>
		public IModel Instance { get; private set; }
		/// <summary>
		/// TODO comment
		/// </summary>
		private readonly string exchange;


		/// <summary>
		/// TODO comment
		/// </summary>
		public Channel(IModel channel, string exchange) {
			this.Instance = channel;
			this.exchange = exchange;
		}

		/// <summary>
		/// Send RPC request
		/// </summary>
		public Task SendRpc(string topic, byte[] payload, string correlationId) {
			var props = Instance.CreateBasicProperties();

			props.CorrelationId = correlationId;
			props.ReplyTo = ServiceConstants.REPLY_QUEUE_NAME;

			return Task.Run(()=>
				Instance.BasicPublish(
					exchange: exchange,
					routingKey: topic,
					basicProperties: props,
					body: payload)
			);

		}

		/// <summary>
		/// Send notify message
		/// </summary>
		public Task SendNotify(string topic, byte[] payload) {
			var props = Instance.CreateBasicProperties();
			var notifier = Instance.CreateBasicPublishBatch();

			return Task.Run(() =>
				Instance.BasicPublish(
					exchange: exchange,
					routingKey: topic,
					basicProperties: props,
					body: payload)
			);
		}
	}
}