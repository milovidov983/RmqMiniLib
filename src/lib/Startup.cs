using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RmqLib.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib {
	public class Startup {
		public static void Init(
			IServiceCollection services,
			IServiceProvider serviceProvider,
			IConnectionFactory connectionFactory,
			RmqConfig config, 
			ILogger logger) {
			try {
				ExecuteInit(services, serviceProvider, connectionFactory, config, logger);
			} catch(Exception e) {
				throw new RmqException($"Rmq initialization error: {e.Message}", e, Error.INTERNAL_ERROR);
			}
		}

		private static void ExecuteInit(IServiceCollection services, IServiceProvider serviceProvider, IConnectionFactory connectionFactory, RmqConfig config, ILogger logger) {
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
			var channel = channelFactory.Create(responseHandler);
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
				var commandImplementations = commands
								.ConvertAll(c => (IRmqCommandHandler)serviceProvider.GetService(c));

				var notificationImplementations = notificationHandlers
					.ConvertAll(c => (IRmqNotificationHandler)serviceProvider.GetService(c));


				// создать класс инициализатор команд обработчиков
				var commandsManager = new CommandHandlersManager(
					commandImplementations, 
					notificationImplementations);
				
				// Создать обработчик всех входящих запросов к микросервису из шины
				// Инициализировать каналом и обработчиками
				// TODO put consumer exception handler
				var requestHandelr = new RequestHandler(logger, config.AppId, channel, commandsManager);

				var topics = commandsManager.GetAllTopics();
				// Запустить прослушивание топиков
				channelFactory.BindRequestHandler(topics.ToList(), requestHandelr);
			}
		}
	}
}