using ServerExample.Service.Infrastructure;
using System;
using System.Threading.Tasks;

namespace ServerExample.Service {
	class Program {
		private static ILogger logger;

		static async Task Main(string[] args) {
			logger = new Logger();
			try {
				await StartService();
			} catch(Exception e) {
				logger.Fatal(e, "Некорректное завершение сервиса.");
			}
		}

		private static async Task StartService() {
			var exitEvent = new System.Threading.ManualResetEvent(false);
			Console.CancelKeyPress += (s, e) => {
				e.Cancel = true;
				exitEvent.Set();
			};

			var startup = new Startup();
			var settings = startup.LoadSettings();

			var hub = startup.CreateHub();

			logger.Info("Запуск сервиса");
			var mqp = new MessageQueueProcessor(hub, settings, logger);
			
			logger.Info("Обработчики запущены");
			await mqp.Start();
			
			logger.Info("Сервис запущен.");
			exitEvent.WaitOne();

			logger.Info("Завершение работы сервиса.");
			mqp.Stop();
		}
	}
}
