using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExampleContracts1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RmqLib;

namespace ExampleService2.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class HealthCheck : ControllerBase {
		private readonly ILogger<HealthCheck> _logger;
		private readonly IRmqSender rmqSender;

		public HealthCheck(ILogger<HealthCheck> logger, IRmqSender rmqSender) {
			_logger = logger;
			this.rmqSender = rmqSender;
		}

		[HttpGet]
		public async Task<int> Get() {
			var r = await rmqSender.Send<SumExampleCommand.Request, SumExampleCommand.Response>(SumExampleCommand.Topic,
				new SumExampleCommand.Request {
					A = 10,
					B = 2
				});

			Console.WriteLine("Result " + r);
			return r.Sum;
		}
	}
}
