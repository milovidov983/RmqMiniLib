using Microsoft.Extensions.Configuration;
using RmqLib;
using System;

namespace ServerExample.Service {
	public class Startup {
		private Settings settings = new Settings();
		private string env;

		public Settings LoadSettings() {
			env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

			if (string.IsNullOrWhiteSpace(env)) {
				throw new Exception("ASPNETCORE_ENVIRONMENT env variable not set.");
			}

			var builder = new ConfigurationBuilder()
				.AddJsonFile("settings.json", true, true);

			var configuration = builder.Build();
			configuration.Bind(settings);

			settings.Env = env;
			
			return settings;
		}

		public IRabbitHub CreateHub() {
			var rmqConfig = settings.RmqConfig;
			Console.Title = $"{rmqConfig.AppId} {env}";
			Console.WriteLine($"{rmqConfig.AppId} {env}");

			return new RabbitHub(rmqConfig);
		}


	}
}
