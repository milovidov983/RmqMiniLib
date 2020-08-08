using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RmqLib.Core;
using System;
using System.Collections.Generic;
using System.Text;
using IConnection = RmqLib.IConnection;

namespace RmqLib {
	/// <summary>
	/// TODO comment
	/// </summary>
	internal class ChannelFactory: IChannelFactory {
		/// <summary>
		/// TODO comment
		/// </summary>
		private readonly IConnection connection;
		/// <summary>
		/// TODO comment
		/// </summary>
		private readonly RmqConfig rmqConfig;

		/// <summary>
		/// TODO comment
		/// </summary>
		public ChannelFactory(IConnection connection, RmqConfig rmqConfig) {
			this.connection = connection;
			this.rmqConfig = rmqConfig;
		}
		/// <summary>
		/// TODO comment
		/// </summary>
		public IChannel Create(IReplyHandler handler) {
			var channel = connection.RmqConnection.CreateModel();

			channel.ExchangeDeclare(rmqConfig.Exchange, ExchangeType.Topic, durable: true);
			channel.ExchangeDeclare(ServiceConstants.FANOUT_EXCHANGE, ExchangeType.Fanout, durable: true);

			if (rmqConfig.Queue != null) {
				channel.QueueDeclare(
					queue: rmqConfig.Queue,
					durable: true,
					exclusive: false,
					autoDelete: false,
					arguments: null);
			}
			channel.BasicQos(
				prefetchSize: 0,
				prefetchCount: rmqConfig.PrefetchCount,
				global: false);

			BindReplyHandler(channel, handler);

			return new Channel(channel, rmqConfig.Exchange);
		}

		private void BindReplyHandler(IModel channel, IReplyHandler handler) {
			var consumer = new AsyncEventingBasicConsumer(channel);
			channel.BasicConsume(
				consumer: consumer,
				queue: ServiceConstants.REPLY_QUEUE_NAME,
				autoAck: true);
			consumer.Received += handler.ReceiveReply;
		}

	}
}
