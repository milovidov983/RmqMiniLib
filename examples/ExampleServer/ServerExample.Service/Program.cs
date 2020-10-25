using Microsoft.Extensions.Configuration;
using RmqLib;
using ServerExample.Service.Infrastructure;
using System;
using System.Threading.Tasks;

namespace ServerExample.Service {
	class Program {
		static async Task Main(string[] args) {
			var exitEvent = new System.Threading.ManualResetEvent(false);
			Console.CancelKeyPress += (s, e) => {
				e.Cancel = true;
				exitEvent.Set();
			};

			var startup = new Startup();
			var hub = startup.CreateHub();

			ILogger logger = new Logger();
			logger.Info("Запуск сервиса");
			var settings = new Settings();
			var mqp = new MessageQueueProcessor(hub, settings, logger);
			logger.Info("Обработчики запущены");
			await mqp.Start();
			logger.Info("Сервис запущен.");
			exitEvent.WaitOne();
			logger.Info("Завершение работы сервиса.");
			mqp.Stop();

		}
	}



	class Startup {
		private RmqConfig rmqConfigInstance;

		public IRabbitHub CreateHub() {
			var builder = new ConfigurationBuilder()
					.AddJsonFile("settings.json", true, true);

			var configuration = builder.Build();
			rmqConfigInstance = new RmqConfig();
			var settingsSection = configuration.GetSection(nameof(RmqConfig));
			settingsSection.Bind(rmqConfigInstance);

			Console.Title = $"{rmqConfigInstance.AppId}";
			Console.WriteLine($"{rmqConfigInstance.AppId} loading...........................");

			return new RabbitHub(rmqConfigInstance);
		}


	}
}
