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

		[HttpGet, Route("ExecuteRpc")]
		public async Task<string> ExecuteRpc() {
			var result = await hub.Send<SC.GetMessageExample.Request, SC.GetMessageExample.Response>(SC.GetMessageExample.Topic,
				new SC.GetMessageExample.Request {
					Id = 1
				});
			return result.Data;
		}

		[HttpGet, Route("SendNotify")]
		public async Task Notify() {
			await hub.Notify(SC.NotifyExample.Topic,
				new SC.NotifyExample.Message {
					Data = "NOTIFICATION EXAMPLE DATA"
				});
		}
	}
}
