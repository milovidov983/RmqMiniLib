﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RmqLib.Core;
using System;
using System.Linq;
using System.Reflection;

namespace RmqLib {
	/// <summary>
	/// Класс инициализирует rmq подключение и привязывает пользовательские сервисы к командам
	/// </summary>
	public class Startup {
		public static void Init(
			IServiceCollection services,
			RmqConfig config) {
			try {
				var serviceProvider = services.BuildServiceProvider();
				var logger = serviceProvider.GetService<ILogger>();
				var connectionEvents = CreateConnectionEvents(services, serviceProvider);
				var connectionFactory = new ConnectionFactory(config, connectionEvents, logger);

				ExecuteInit(services, connectionFactory, config, logger);
			} catch (Exception e) {
				throw new RmqException($"Rmq initialization error: {e.Message}", e, Error.INTERNAL_ERROR);
			}
		}

		private static ConnectionEvents CreateConnectionEvents(IServiceCollection services, IServiceProvider serviceProvider) {
			IEventHandlersFactoryService eventHandlersFactoryService = null;
			if (services.Any(x => x.ServiceType == typeof(IEventHandlersFactoryService))) {
				eventHandlersFactoryService = serviceProvider.GetService<IEventHandlersFactoryService>();
			}
			return new ConnectionEvents(eventHandlersFactoryService);
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

			logger?.LogInformation($"Try to find commands handlers services..");
			// - если у сервиса есть подходящие команды то инициализировать их
			var commands = Assembly.GetEntryAssembly()
					.GetTypes()
					.Where(p => typeof(IRmqCommandHandler).IsAssignableFrom(p) && !p.IsInterface)
					.ToList();

			if (commands.Any()) {
				logger?.LogInformation($"Found {commands.Count} services, try to add the services to DI");

				commands.ForEach(c => services.AddSingleton(c));
				logger?.LogInformation($"The services added to DI successfully");

				var serviceProvider = services.BuildServiceProvider();
				var commandImplementations = commands
								.ConvertAll(c => (IRmqCommandHandler)serviceProvider.GetService(c));

				// - Создать класс инициализатор команд обработчиков
				var commandsManager = new CommandHandlersManager(commandImplementations);

				// - Создать обработчик всех входящих запросов к микросервису из шины. Инициализировать каналом и обработчиками
				var requestHandelr = new RequestHandler(config.AppId, channel,commandsManager, sender);

				var topics = commandsManager.GetAllTopics();

				// - Запустить прослушивание топиков
				channelFactory.BindRequestHandler(topics.ToList(), requestHandelr);
			} else {
				logger?.LogInformation($"Handler commands implementing {nameof(IRmqCommandHandler)} interface not found");
			}
		}
	}
}