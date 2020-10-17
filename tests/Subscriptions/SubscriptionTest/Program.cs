using Microsoft.Extensions.Configuration;
using RmqLib;
using System;

namespace SubscriptionTest {
	class Program {
		static void Main(string[] args) {
			var startup = new Startup();
			var hub = startup.Init();
			var subs = hub.CreateSubscriptions();
			Console.WriteLine("Subscriptions init");
			Console.ReadKey();// ("Hello World!");
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
}
