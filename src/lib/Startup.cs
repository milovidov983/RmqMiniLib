using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RmqLib.Core;
using RmqLib.Factories;
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
			RmqConfig config) {
			try {
				IServiceProvider serviceProvider = services.BuildServiceProvider();
				IEventHandlersFactoryService eventHandlersFactoryService = null;
				if (services.Any(x => x.ServiceType == typeof(IEventHandlersFactoryService))) {
					eventHandlersFactoryService = serviceProvider.GetService<IEventHandlersFactoryService>();
				}
				var connectionEvents = new ConnectionEvents(eventHandlersFactoryService);
				var logger = serviceProvider.GetService<ILogger>();

				var connectionFactory = new ConnectionFactory(config, connectionEvents, logger);

				ExecuteInit(services, connectionFactory, config, logger);
			} catch(Exception e) {
				throw new RmqException($"Rmq initialization error: {e.Message}", e, Error.INTERNAL_ERROR);
			}
		}

		private static void ExecuteInit(IServiceCollection services, IConnectionFactory connectionFactory, RmqConfig config, ILogger logger) {
			// 1. create connection
			var connection = connectionFactory.Create();

			logger?.LogInformation($"Try connect to RabbitMQ host {config?.HostName}...");
			connection.StartConnection();
			logger?.LogInformation($"RabbitMQ host {config?.HostName} connection established successfully.");

			// 2. create channel
			var channelFactory = new ChannelFactory(connection, config);

			logger?.LogInformation($"Try to bind channel and declare the work exchanges and the queue {config?.Queue}.");
			// - create response handler
			var responseHandler = new ResponseHandelr();

			// - получить все топики
			var channel = channelFactory.Create(responseHandler);
			logger?.LogInformation($"Channel binded to {config?.Queue} successfully");


			// - добавить как singleton сервис rmqSender
			var sender = new RmqSender(config, responseHandler, channel);
			services.AddSingleton<IRmqSender, RmqSender>(factory => sender);



			// - если у сервиса есть подходящие команды то инициализировать их
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

				var serviceProvider = services.BuildServiceProvider();
				var commandImplementations = commands
								.ConvertAll(c => (IRmqCommandHandler)serviceProvider.GetService(c));

				var notificationImplementations = notificationHandlers
					.ConvertAll(c => (IRmqNotificationHandler)serviceProvider.GetService(c));


				// - Создать класс инициализатор команд обработчиков
				var commandsManager = new CommandHandlersManager(
					commandImplementations, 
					notificationImplementations);
				
				// - Создать обработчик всех входящих запросов к микросервису из шины. Инициализировать каналом и обработчиками
				// TODO put consumer exception handler
				var requestHandelr = new RequestHandler(logger, config.AppId, channel, commandsManager);


				var topics = commandsManager.GetAllTopics();

				// - Запустить прослушивание топиков
				channelFactory.BindRequestHandler(topics.ToList(), requestHandelr);
			}


		}
	}
}