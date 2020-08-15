using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RmqLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IConnection = RmqLib.IConnection;

namespace RmqLib {
	/// <summary>
	/// TODO comment
	/// </summary>
	internal class ChannelFactory : IChannelFactory {
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
		public IChannel Create(IReplyHandler handler, string[] topics = null) {
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
			if (topics != null) {
				


				BindCommandHandlers(channel, rmqConfig, topics.ToList());
			}

			return new Channel(channel, rmqConfig.Exchange);
		}

		private void BindCommandHandlers(IModel channel, RmqConfig rmqConfig, List<string> topics) {
			var consumer = new AsyncEventingBasicConsumer(channel);

			// TODO не забыть что есть такие события 
			// вероятно стоит с ними работать ради большей стабильности либы
			consumer.Received += mainHandler.Handle; // надо как то создать обработчик на этой стадии
			//consumer.Shutdown += OnConsumerShutdown;
			//consumer.Registered += OnConsumerRegistered;
			//consumer.Unregistered += OnConsumerUnregistered;
			//consumer.ConsumerCancelled += OnConsumerCancelled;

			topics.ForEach(topic => {
				if (IsCommandHandlerTopic(topic)) {
					channel.QueueBind(
						queue: rmqConfig.Queue,
						exchange: rmqConfig.Exchange,
						routingKey: topic,
						arguments: null);
				} else if (IsNotificationHandlerTopic(topic)) {
					channel.QueueBind(
						queue: rmqConfig.Queue,
						exchange: ServiceConstants.FANOUT_EXCHANGE,
						routingKey: topic,
						arguments: null);

				} else {
					throw new RmqException($"{topic}: incorrect name for the topic, "
						+ $"the name must end with \"{ServiceConstants.PPC_TOKEN_TOPIC}\" " +
						$"or \"{ServiceConstants.NOTIFICATION_TOKEN_TOPIC}\"!", Error.INTERNAL_ERROR);
				}
			});

			channel.BasicConsume(
				queue: rmqConfig.Queue,
				autoAck: false,
				consumer: consumer);
		}


		private void BindReplyHandler(IModel channel, IReplyHandler handler) {
			var consumer = new AsyncEventingBasicConsumer(channel);
			channel.BasicConsume(
				consumer: consumer,
				queue: ServiceConstants.REPLY_QUEUE_NAME,
				autoAck: true);
			consumer.Received += handler.ReceiveReply;
		}

		private static bool IsCommandHandlerTopic(string topic) {
			return topic.EndsWith(ServiceConstants.PPC_TOKEN_TOPIC);
		}
		private static bool IsNotificationHandlerTopic(string topic) {
			return topic.EndsWith(ServiceConstants.NOTIFICATION_TOKEN_TOPIC);
		}
	}
}
