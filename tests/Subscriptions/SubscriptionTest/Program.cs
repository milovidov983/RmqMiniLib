using Microsoft.Extensions.Configuration;
using RmqLib;
using SubscriptionTestContract;
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
				.AddHandlers(cfg => 
								cfg
								.OnException((exc, dm) => {
									Console.WriteLine($"Error on MessageProcessor. {exc.Message}");

									if (dm.IsRpcMessage()) {
										hub.SetRpcErrorAsync(dm, exc.Message, (int)StatusCodes.InternalError);
									}

									return Task.FromResult(false);
								})
								.OnUnexpectedTopic(async dm => {
									var message = $"{dm.GetTopic()} was unexpected";
									Console.WriteLine(message);


									if (dm.IsRpcMessage()) {
										await hub.SetRpcErrorAsync(dm, message, (int)StatusCodes.InvalidRequest);
									}

									return await MessageProcessResult.RejectTask;
								})
								.OnTopic(ExampleClass.Topic, new CommandBase()))
								.Start();




			Console.WriteLine("Subscriptions init");
			Console.ReadKey();
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
