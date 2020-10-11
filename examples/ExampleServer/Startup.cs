using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RmqLib;
using Server.Services;

namespace ExampleServer {



	public class Startup {

		public Startup(IConfiguration configuration, IWebHostEnvironment env) {
			Configuration = configuration;
			Console.WriteLine(env.EnvironmentName);
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", true, true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
				.AddEnvironmentVariables("ExampleServer:");

			Configuration = builder.Build();
			RmqConfig = new RmqConfig();
			var settingsSection = Configuration.GetSection(nameof(RmqConfig));
			settingsSection.Bind(RmqConfig);

			Console.Title = $"{RmqConfig.AppId} {env.EnvironmentName}";
		}

		public IConfiguration Configuration { get; }
		public RmqConfig RmqConfig { get; }


		public void ConfigureServices(IServiceCollection services) {
			services.AddControllers();
			services.AddOptions();
			services.AddLogging();

			services.AddSingleton<DatabaseService>();

			Console.WriteLine("Before RabbitMQ init");
			RmqLib.Startup.Init(services, RmqConfig);
			Console.WriteLine("RMQ connected");
		}


		public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseEndpoints(endpoints => {
				endpoints.MapControllers();
			});
		}
	}
}
