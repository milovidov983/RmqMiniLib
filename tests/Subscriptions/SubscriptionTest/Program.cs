using Microsoft.Extensions.Configuration;
using RmqLib;
using System;
using System.Threading.Tasks;

namespace SubscriptionTest {

	public class CommandBase : DefaultRabbitCommand {
		public override async Task<MessageProcessResult> Execute(DeliveredMessage dm) {
			var req = dm.GetContent<ExampleClass.Request>();

			Console.WriteLine($"Message get! {req.Message}");



			await Hub.SetRpcResultAsync(dm, new ExampleClass.Response { Message = "server set rpc result 42!" });

			return MessageProcessResult.Ack;
		}
	}


	class Program {
		static void Main(string[] args) {
			var startup = new Startup();
			var hub = startup.Init();

			var subs = hub.DefineHandlers()
				.ForQueue("", 
					cfg => cfg
					.OnTopic(ExampleClass.Topic, new CommandBase()))
				.Start();




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
