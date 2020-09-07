using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RmqLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RmqLib {
	/// <summary>
	/// TODO comment
	/// </summary>
	internal class ChannelFactory : IChannelFactory {
		/// <summary>
		/// TODO comment
		/// </summary>
		private readonly IConnectionService connection;
		/// <summary>
		/// TODO comment
		/// </summary>
		private readonly RmqConfig rmqConfig;
		/// <summary>
		/// TODO comment
		/// </summary>
		private IModel channel;

		/// <summary>
		/// TODO comment
		/// </summary>
		public ChannelFactory(IConnectionService connection, RmqConfig rmqConfig) {
			this.connection = connection;
			this.rmqConfig = rmqConfig;
		}
		/// <summary>
		/// TODO comment
		/// </summary>
		public IChannel Create(IReplyHandler handler) {
			this.channel = connection.RmqConnection.CreateModel();

			DeclareExchanges(channel);
			DeclareQueue(channel);
			ConfiguresQoS(channel);
			BindReplyHandler(channel, handler);

			return new Channel(channel, rmqConfig.Exchange);
		}
		//TODO comment
		private void DeclareExchanges(IModel channel) {
			channel.ExchangeDeclare(rmqConfig.Exchange, ExchangeType.Topic, durable: true);
		}
		//TODO comment
		private void DeclareQueue(IModel channel) {
			if (rmqConfig.Queue != null) {
				channel.QueueDeclare(
					queue: rmqConfig.Queue,
					durable: true,
					exclusive: false,
					autoDelete: false,
					arguments: null);
			}
		}
		//TODO comment
		private void ConfiguresQoS(IModel channel) {
			channel.BasicQos(
				prefetchSize: 0,
				prefetchCount: rmqConfig.PrefetchCount,
				global: false);
		}


		/// <summary>
		/// Привязать обработчик входящих запросов
		/// </summary>
		/// <param name="topics">список топиков</param>
		/// <param name="requestHandler">Обработчик входящих команд из шины</param>
		public void BindRequestHandler(List<string> topics, IRequestHandler requestHandler) {
			if(topics?.Any() != true) {
				return;
			}

			var consumer = new AsyncEventingBasicConsumer(channel);

			consumer.Received += requestHandler.Handle; // надо как то создать обработчик на этой стадии

			// TODO не забыть что есть такие события 
			// вероятно стоит с ними работать ради большей стабильности либы
			//consumer.Shutdown += OnConsumerShutdown;
			//consumer.Registered += OnConsumerRegistered;
			//consumer.Unregistered += OnConsumerUnregistered;
			//consumer.ConsumerCancelled += OnConsumerCancelled;

			topics.ForEach(topic => {
				channel.QueueBind(
					queue: rmqConfig.Queue,
					exchange: rmqConfig.Exchange,
					routingKey: topic,
					arguments: null);
				
			});

			channel.BasicConsume(
				queue: rmqConfig.Queue,
				autoAck: false,
				consumer: consumer);
		}

		/// <summary>
		/// TODO comment
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="handler"></param>
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
