using Microsoft.Extensions.Configuration;
using RmqLib2;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TestRmqSenderProject {

	class RequestResponse {
		public string Message { get; set; }
	}

	class Program {
		public const string Topic = "example.topic.rpc";
		static async Task Main(string[] args) {

			var rand = new Random((int)DateTime.UtcNow.Ticks);

			var startup = new Startup();

			var hub = startup.Init();

			var tasks = Enumerable.Range(1, 10).Select((x) => Task.Run(async () => {
				try {
					var delayMs = rand.Next(1000, 4000);
					Console.WriteLine($"[{x}] Start delay {delayMs}");

					await Task.Delay(delayMs);

					Console.WriteLine($"[{x}] Start process");
					var response = await hub.ExecuteRpcAsync<RequestResponse, RequestResponse>(
						Topic,
						new RequestResponse {
							Message = $"hello! {x}"
						}
					);

					Console.WriteLine($"[{x}] Resp {response.Message}");
				} catch (Exception e) {
					Console.WriteLine(e.Message);
				}

			})).ToArray();

			await Task.WhenAll(tasks);


			Console.ReadKey();
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
			Console.WriteLine($"...........................");

			var hub = new RabbitHub(rmqConfigInstance);
			return hub;
		}


	}
}
