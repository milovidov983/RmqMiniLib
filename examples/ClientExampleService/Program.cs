using Microsoft.Extensions.Configuration;
using RmqLib;
using System;
using System.Threading.Tasks;

using SEC = ServerExample.Contracts;

namespace ClientExampleService {
	class Program {
		static async Task Main(string[] args) {
			var startup = new Startup();
			var hub = startup.CreateHub();


			while (true) {
				await SendTestRpcRequest(hub);
				Console.ReadKey();
			}
		}


		private static async Task SendTestRpcRequest(IRabbitHub hub) {
			try {
				var response = await hub.ExecuteRpcAsync<SEC.ExampleCommand.Response, SEC.ExampleCommand.Request>(
					SEC.ExampleCommand.Topic,
					new SEC.ExampleCommand.Request {
						Message = "Hello from ClientExampleService!"
					}
				);

				Console.WriteLine($"Получен ответ от микросервиса: {response.Message}");
			} catch (Exception e) {
				Console.WriteLine(e.Message);
			}
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
