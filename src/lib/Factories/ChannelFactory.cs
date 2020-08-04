using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using IConnection = RmqLib.IConnection;

namespace RmqLib {
	/// <summary>
	/// TODO comment
	/// </summary>
	public class ChannelFactory: IChannelFactory {
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
		public IChannel Create() {
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

			return new Channel(channel, rmqConfig.Exchange);
		}
	}
}
