using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RmqLib;
using SC = Server.Contracts;
namespace ExampleClient.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class ExampleController : ControllerBase {
		private readonly ILogger<ExampleController> _logger;
		private readonly IRmqSender hub;

		public ExampleController(ILogger<ExampleController> logger, IRmqSender hub) {
			_logger = logger;
			this.hub = hub;
		}

		[HttpGet]
		public async Task<string> Execute() {
			var result = await hub.Send<SC.SumExampleCommand.Request, SC.SumExampleCommand.Response>(SC.SumExampleCommand.Topic,
				new SC.SumExampleCommand.Request {
					Id = 1
				});

			System.Diagnostics.Debug.WriteLine(result.Data);

			return result.Data;
		}
	}
}
