﻿using Microsoft.Extensions.DependencyInjection;
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
				throw new RmqException($"Microservice initialization error: {e.Message}", e, Error.INTERNAL_ERROR);
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

			var requestHandelr = new RequestHandler(logger, config.AppId); // TODO put consumer exception handler

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
					notificationImplementations,
					config);

				var topics = commandsManager.GetAllTopics();
				// Инициализировать каналом и обработчиками класс отвечающий за прием сообщений из шины
				requestHandelr.Init(channel.ChannelInstance, commandsManager);
				// Запустить прослушивание топиков
				channelFactory.BindRequestHandler(topics, requestHandelr);
			}
		}
	}

	public class CommandHandlersManager: ICommands {
		private readonly ConcurrentDictionary<string, IRmqNotificationHandler> notifyHandlers;
		private readonly ConcurrentDictionary<string, IRmqCommandHandler> commandsHandlers;

		private readonly string[] allTopics;
		private RmqConfig config;

		public CommandHandlersManager(
			List<IRmqCommandHandler> commandImplementations, 
			List<IRmqNotificationHandler> notificationImplementations, 
			RmqConfig config) {
			this.config = config;

			var cmdHandlers = commandImplementations.ToDictionary(
				x => x.Topic,
				x => x
				);

			var notifHandlers = notificationImplementations.ToDictionary(
				x => x.Topic,
				x => x
				);

			allTopics = cmdHandlers.Keys.Union(notifHandlers.Keys).ToArray();


			commandsHandlers = new ConcurrentDictionary<string, IRmqCommandHandler>(cmdHandlers);
			notifyHandlers = new ConcurrentDictionary<string, IRmqNotificationHandler>(notifHandlers);
		}

		public IRmqHandler GetHandler(string topic) {
			throw new NotImplementedException();
		}

		public string[] GetAllTopics() {
			throw new NotImplementedException();
		}
	}
}
