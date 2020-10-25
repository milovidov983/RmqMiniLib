using Microsoft.Extensions.Configuration;
using RmqLib;
using System;
using System.Linq;
using System.Threading.Tasks;

using STC = SubscriptionTest.Contract;

namespace TestRmqSenderProject {

	class RequestResponse {
		public string Message { get; set; }
	}

	class Program {
		public const string RpcTopic = "test.topic.rpc";
		public const string NotifyTopic = "exampleNotify.topic.none";
		static async Task Main(string[] args) {
			var startup = new Startup();
			var hub = startup.Init();


			while (true) {
				await TestRpc2(hub);
				Console.ReadKey();
			}
			//await TestNotify(hub);

			Console.ReadKey();
		}

		private static async Task TestNotify(IRabbitHub hub) {
			var tasks = Enumerable.Range(1, 1000).Select((x) => Task.Run(async () => {
				try {
					Console.WriteLine("[x] Send notify ...");
					await hub.PublishAsync(
						NotifyTopic,
						new RequestResponse {
							Message = $"hello notify {x}"
						});
					Console.WriteLine($"[{x}] Notify sent");
				} catch (Exception e) {

					Console.WriteLine($"[{x}] notify error {e.Message}");
				}

			})).ToArray();

			await Task.WhenAll(tasks);
		}

		private static async Task TestRpc(IRabbitHub hub) {
			//var rand = new Random((int)DateTime.UtcNow.Ticks);
			var tasks = Enumerable.Range(1, 1000).Select((x) => Task.Run(async () => {
				try {
					int delayMs = 0;// rand.Next(1000, 4000);
					Console.WriteLine($"[{x}] Start delay {delayMs}");

					await Task.Delay(delayMs);

					Console.WriteLine($"[{x}] Start process");
					var response = await hub.ExecuteRpcAsync<RequestResponse, RequestResponse>(
						RpcTopic,
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
		}		
		
		private static async Task TestRpc2(IRabbitHub hub) {

				try {


					Console.WriteLine($"[] Start process");
					var response = await hub.ExecuteRpcAsync<STC.ExampleClass.Response, STC.ExampleClass.Request>(
					STC.ExampleClass.Topic,
					new STC.ExampleClass.Request {
						Message = "hello from sender"
					}
					); 

					Console.WriteLine($"Resp {response.Message}");
				} catch (Exception e) {
					Console.WriteLine(e.Message);
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
