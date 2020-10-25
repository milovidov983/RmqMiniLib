﻿using Microsoft.Extensions.Configuration;
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
		static async Task Main(string[] args) {
			var startup = new Startup();
			var hub = startup.Init();


			while (true) {
				await SendTestRpcRequest(hub);
				Console.ReadKey();
			}
		}

		
		private static async Task SendTestRpcRequest(IRabbitHub hub) {
				try {
					var response = await hub.ExecuteRpcAsync<STC.ExampleClass.Response, STC.ExampleClass.Request>(
						STC.ExampleClass.Topic,
						new STC.ExampleClass.Request {
							Message = "hello from sender"
						}
					); 

					Console.WriteLine($"Resp from microservice {response.Message}");
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
