using Microsoft.Extensions.Configuration;
using RmqLib2;
using System;
using System.Threading.Tasks;

namespace TestRmqSenderProject {

	class RequestResponse {
		public string Message { get; set; }
	}

	class Program {
		public const string Topic = "example.topic.rpc";
		static async Task Main(string[] args) {

			var startup = new Startup();

			var hub = startup.Init();

			while (true) {
				try {
					var response = await hub.ExecuteRpcAsync<RequestResponse, RequestResponse>(
						Topic,
						new RequestResponse {
							Message = "hello!"
						}
					);

					Console.WriteLine(response);
					Console.ReadKey();
				}catch(Exception e) {
					Console.WriteLine(e.Message);
				}
			}
		}
	}



	class Startup {
		private RmqConfig rmqConfigInstance;

		public IRabbitHub Init() {
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
