using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace RmqLib {
	public class Startup {
		public static void Init(
			IServiceCollection services, 
			IConnectionFactory connectionFactory, 
			RmqConfig config, 
			ILogger logger) {

			var connection = connectionFactory.Create();

			logger?.LogInformation($"Try connect to RabbitMQ host {config?.HostName}...");
			connection.ConnectToRmq();
			logger?.LogInformation($"RabbitMQ host {config?.HostName} connection established successfully.");

			var channelFactory = new ChannelFactory(connection, config);

			logger?.LogInformation($"Try to bind channel and declare the work exchanges and the queue {config?.Queue}.");
			var channel = channelFactory.Create();
			logger?.LogInformation($"Channel binded to {config?.Queue} successfully");


			// 1. create connection
			// 2. create channel
			// ok  теперь допилить ResponseHandelr с упрощением, не разделять обработчик и инициализацию
			// 3. create response handler
			// 4. если у сервиса есть подходящие команды то инициализировать их
			// 5. привязать пользовательские команды к топикам
			// 6. добавить как singleton сервис rmqSender
		}
	}
}
