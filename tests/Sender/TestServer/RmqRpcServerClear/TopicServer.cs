using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RmqRpcServerClear {

	class RequestResponse {
		public string Message { get; set; }
	}

	class TopicServer {
		public const string RpcTopic = "example.topic.rpc";
		public const string NotifyTopic = "exampleNotify.topic.none";
		private IConnection connection;
		private IModel channel;

		public void Start() {
			var factory = new ConnectionFactory() { HostName = "localhost" };
			connection = factory.CreateConnection();
			channel = connection.CreateModel();
			channel.ModelShutdown += (o, e) => Console.WriteLine($"ModelShutdown {e.Cause}");

			connection.ConnectionShutdown += (o, e) => {
				Console.WriteLine($"ConnectionShutdown {e.Cause}");
			};

			channel.ExchangeDeclare(exchange: "my_exchange", type: "topic", true);

			var queueName = "q_example42";

			channel.QueueDeclare(
				queueName,
				durable: true,
				exclusive: false,
				autoDelete: false);



			Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");

			var consumer = new EventingBasicConsumer(channel); ;

			consumer.Shutdown += (o, e) => {
				Console.WriteLine($"consumer.Shutdown {e.ReplyText}");
			};
			consumer.Registered += (o, e) => Console.WriteLine($"consumer.Registered");

			var semaphore = new SemaphoreSlim(1);

			consumer.Received += Handler;



			channel.BasicConsume(queue: queueName,
									autoAck: false,
									consumer: consumer);

			channel.QueueBind(queue: queueName,
						exchange: "my_exchange",
						routingKey: RpcTopic);

			channel.QueueBind(queue: queueName,
				exchange: "my_exchange",
				routingKey: NotifyTopic);

			Console.ReadLine();


			async Task HandlerAsync(object model, BasicDeliverEventArgs ea) {

				var body = ea.Body.ToArray();
				var props = ea.BasicProperties;

				await semaphore.WaitAsync();
				var replyProps = channel.CreateBasicProperties();
				semaphore.Release();
				replyProps.CorrelationId = props.CorrelationId;

				RequestResponse response = new RequestResponse();
				try {
					var json = Encoding.UTF8.GetString(body);
					var resp = JsonSerializer.Deserialize<RequestResponse>(json);
					var routingKey = ea.RoutingKey;
					Console.WriteLine($"{routingKey} Received message: {resp.Message}");

					if (resp.Message == "hello! 42") {
						Console.WriteLine("Sleep...");
						await Task.Delay(3000);
					}

					response.Message = "ok";

				} catch (Exception e) {
					Console.WriteLine(" [.] " + e.Message);

					response.Message = "";
				} finally {
					var json = JsonSerializer.Serialize(response);
					var responseBytes = Encoding.UTF8.GetBytes(json);

					await semaphore.WaitAsync();
					channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: responseBytes);
					channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
					semaphore.Release();
				}
			}

			void Handler(object model, BasicDeliverEventArgs ea) {
				var body = ea.Body.ToArray();
				var props = ea.BasicProperties;
				var routingKey = ea.RoutingKey;
				var dt = ea.DeliveryTag;
				Task.Factory.StartNew(async () => {

					await semaphore.WaitAsync();
					var replyProps = channel.CreateBasicProperties();
					semaphore.Release();
					replyProps.CorrelationId = props.CorrelationId;

					RequestResponse response = new RequestResponse();
					try {
						var json = Encoding.UTF8.GetString(body);
						var resp = JsonSerializer.Deserialize<RequestResponse>(json);

						Console.WriteLine($"{routingKey} Received message: {resp.Message}");

						if (resp.Message == "hello! 42") {
							Console.WriteLine("Sleep...");
							await Task.Delay(3000);
						}

						response.Message = "ok";

					} catch (Exception e) {
						Console.WriteLine(" [.] " + e.Message);

						response.Message = "";
					} finally {
						try {
							if (routingKey != NotifyTopic) {
						
								var json = JsonSerializer.Serialize(response);
								var responseBytes = Encoding.UTF8.GetBytes(json);

								await semaphore.WaitAsync();
								channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: responseBytes);
								channel.BasicAck(deliveryTag: dt, multiple: false);
							}

						} catch (Exception e) {
							Console.WriteLine(" [.] " + e.Message);
						} finally {
							semaphore.Release();

						}
					}
				});
			}


			void HandlerSafe(object model, BasicDeliverEventArgs ea) {

				var body = ea.Body.ToArray();
				var props = ea.BasicProperties;


				var replyProps = channel.CreateBasicProperties();

				replyProps.CorrelationId = props.CorrelationId;

				RequestResponse response = new RequestResponse();
				try {
					var json = Encoding.UTF8.GetString(body);
					var resp = JsonSerializer.Deserialize<RequestResponse>(json);
					var routingKey = ea.RoutingKey;
					Console.WriteLine($"{routingKey} Received message: {resp.Message}");

					if (resp.Message == "hello! 42") {
						Console.WriteLine("Sleep...");
					}

					response.Message = "ok";

				} catch (Exception e) {
					Console.WriteLine(" [.] " + e.Message);

					response.Message = "";
				} finally {
					var json = JsonSerializer.Serialize(response);
					var responseBytes = Encoding.UTF8.GetBytes(json);


					channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: responseBytes);
					channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
					semaphore.Release();
				}

			}
		}
	}
}
