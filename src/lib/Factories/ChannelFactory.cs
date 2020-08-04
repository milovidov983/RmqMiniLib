using RabbitMQ.Client;
using RmqLib.Core;
using System;
using System.Collections.Generic;
using System.Text;
using IConnection = RmqLib.Core.IConnection;

namespace RmqLib.Factories {

	public interface IChannel {

	}
	public class Channel : IChannel {
		private readonly IModel channel;
		private readonly string exchange;
		private const string replyQueueName = "amq.rabbitmq.reply-to";
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
			props.ReplyTo = replyQueueName;

			channel.BasicPublish(
				exchange: exchange,
				routingKey: topic,
				basicProperties: props,
				body: payload);

			return correlationId;
		}
	}

	public class ChannelFactory {
		private readonly IConnection connection;
		private readonly RmqConfig rmqConfig;

		public ChannelFactory(IConnection connection, RmqConfig rmqConfig) {
			this.connection = connection;
			this.rmqConfig = rmqConfig;
		}


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
