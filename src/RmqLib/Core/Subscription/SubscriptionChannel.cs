using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib {
	public class SubscriptionChannel {
		private IModel channel;
		private string queueName = "q_testQueue";
		private SemaphoreSlim semaphore = new SemaphoreSlim(1);



		private ConcurrentDictionary<string, Func<DeliveredMessage, Task<MessageProcessResult>>> topicHandlers = new ConcurrentDictionary<string, Func<DeliveredMessage, Task<MessageProcessResult>>>() {
			["test.topic.rpc"] = async (msg) => {
				Console.WriteLine($"Message get! {msg}");
				await Task.Yield();
				return MessageProcessResult.Ack;
			}
		};

		public SubscriptionChannel(IModel channel, ushort prefechCount = 32) {
			this.channel = channel;

			channel.BasicQos(0, prefechCount, false);

			channel.ModelShutdown += (o, e) => Console.WriteLine($"RMQ Channel shutdown {e.Cause}");

			channel.QueueDeclare(
				queueName,
				durable: true,
				exclusive: false,
				autoDelete: false);


			//var consumer = new EventingBasicConsumer(channel);
			var consumer = new AsyncEventingBasicConsumer(channel);

			consumer.Shutdown += async (o, e) => {
				Console.WriteLine($"consumer.Shutdown {e.ReplyText}");
				await Task.Yield();
			};
			consumer.Registered += async (o, e) => {
				Console.WriteLine($"consumer.Registered");
				await Task.Yield();

			};

			consumer.Received += Handler;

			channel.BasicConsume(queue: queueName,
						autoAck: false,
						consumer: consumer);

			foreach (var topic in topicHandlers.Keys) {
				channel.QueueBind(queue: queueName,
							exchange: "my_exchange",
							routingKey: topic);
			}

		}

		Task Handler(object model, BasicDeliverEventArgs ea) {
			var body = ea.Body.ToArray();
			var props = ea.BasicProperties;
			var routingKey = ea.RoutingKey;
			var dt = ea.DeliveryTag;

			DeliveredMessage deliveredMessage = CreateDeliveredMessage(ea);


			return Task.Factory.StartNew(async () => {
				try {
					await ExecuteBeforeExecuteHandler(deliveredMessage);

					var handlerExists = topicHandlers.TryGetValue(routingKey, out var handler);
					if (!handlerExists) {
						MessageProcessResult mpr = await ExecuteUnexpectedTopicHandler(deliveredMessage);
						await Ask(dt, mpr);
						return;
					}

					string content = GetContent(body);
					MessageProcessResult processResult = MessageProcessResult.Reject;
					try {
						processResult = await handler.Invoke(deliveredMessage);
					} catch(Exception e) {
						await ExecuteExceptionHandler(e, deliveredMessage);
					} finally {
						processResult = await ExecuteAfterExecuteHandler(deliveredMessage, processResult);
						await Ask(dt, processResult);
					}

					var resp = "resp";
					byte[] respBody = Encoding.UTF8.GetBytes(resp);

					await semaphore.WaitAsync();
					var replyProps = channel.CreateBasicProperties();
					replyProps.CorrelationId = props.CorrelationId;
					channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: respBody);

				} catch (Exception e) {
					Console.WriteLine(" [.] " + e.Message);
					await semaphore.WaitAsync();
					channel.BasicReject(dt, false);
				} finally {
					semaphore.Release();
				}
			});
		}

		private Task<MessageProcessResult> ExecuteUnexpectedTopicHandler(DeliveredMessage deliveredMessage) {
			return Task.FromResult(MessageProcessResult.Ack);
		}

		private async Task Ask(ulong dt, MessageProcessResult processResult) {
			try {
				await semaphore.WaitAsync();
				switch (processResult) {
					case MessageProcessResult.Ack:
						channel.BasicAck(deliveryTag: dt, multiple: false);
						break;
					case MessageProcessResult.Requeue:
						channel.BasicReject(dt, true);
						break;
					case MessageProcessResult.Reject:
						channel.BasicReject(dt, false);
						break;
				}
			} finally {
				semaphore.Release();
			}
		}

		private Task ExecuteExceptionHandler(Exception e, DeliveredMessage deliveredMessage) {
			return Task.CompletedTask;
		}

		private Task<MessageProcessResult> ExecuteAfterExecuteHandler(DeliveredMessage deliveredMessage, MessageProcessResult processResult) {
			return Task.FromResult(MessageProcessResult.Ack);
		}

		private Task ExecuteBeforeExecuteHandler(DeliveredMessage deliveredMessage) {
			return Task.CompletedTask;
		}

		private DeliveredMessage CreateDeliveredMessage(BasicDeliverEventArgs ea) {
			return null;
		}

		private void ProcessHandlerException(string routingKey, Exception e) {
			SetError($"Handler exception for topic {routingKey} {e.Message}");
		}

		private string GetContent(byte[] body) {
			return System.Text.Encoding.UTF8.GetString(body);
		}


		private void SetError(string v) {
			Console.WriteLine($"Rmq error: {v}");
		}
	}
}
