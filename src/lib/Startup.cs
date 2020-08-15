using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RmqLib.Core;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib {
	public class Startup {
		public static void Init(
			IServiceCollection services, 
			IConnectionFactory connectionFactory,
			RmqConfig config, 
			ILogger logger) {
			try {
				ExecuteInit(services, connectionFactory, config, logger);
			} catch(Exception e) {
				throw new RmqException($"Microservice initialization error: {e.Message}", e, Error.INTERNAL_ERROR);
			}
		}

		private static void ExecuteInit(IServiceCollection services, IConnectionFactory connectionFactory, RmqConfig config, ILogger logger) {
			// 1. create connection
			var connection = connectionFactory.Create();

			logger?.LogInformation($"Try connect to RabbitMQ host {config?.HostName}...");
			connection.ConnectToRmq();
			logger?.LogInformation($"RabbitMQ host {config?.HostName} connection established successfully.");

			// 2. create channel
			var channelFactory = new ChannelFactory(connection, config);

			logger?.LogInformation($"Try to bind channel and declare the work exchanges and the queue {config?.Queue}.");
			// 3. create response handler
			var responseHandler = new ResponseHandelr();

			// 3.1 получить все топики
			string[] allTopics = new string[] { };
			var channel = channelFactory.Create(responseHandler, allTopics);
			logger?.LogInformation($"Channel binded to {config?.Queue} successfully");

			// 4. если у сервиса есть подходящие команды то инициализировать их
			var commands = Assembly.GetEntryAssembly()
					.GetTypes()
					.Where(p => typeof(IRmqCommandHandler).IsAssignableFrom(p) && !p.IsInterface)
					.ToList();

			var notificationHandlers = Assembly.GetEntryAssembly()
				.GetTypes()
				.Where(p => typeof(IRmqNotificationHandler).IsAssignableFrom(p) && !p.IsInterface)
				.ToList();

			if (commands.Any() || notificationHandlers.Any()) {

				commands.ForEach(c => services.AddSingleton(c));
				notificationHandlers.ForEach(c => services.AddSingleton(c));




				// 5. привязать пользовательские команды к топикам
				services.AddHostedService(service => {

					var commandImplementations = commands
									.ConvertAll(c => (IRmqCommandHandler)service.GetService(c));

					var notificationImplementations = notificationHandlers
						.ConvertAll(c => (IRmqNotificationHandler)service.GetService(c));


					// создать класс инициализатор команд обработчиков
					return new CommandHandlersBinder(
						commandImplementations, 
						notificationImplementations,
						config);
				});


			}

			// 6. добавить как singleton сервис rmqSender
			var sender = new RmqSender(config, responseHandler, channel);
			services.AddSingleton<IRmqSender, RmqSender>(factory => sender);
		}
	}

	public class CommandHandlersBinder : BackgroundService {
		protected override Task ExecuteAsync(CancellationToken stoppingToken) {
			throw new NotImplementedException();
		}
	}
}
