using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExampleService1.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class HealthCheck : ControllerBase {
		private readonly ILogger<HealthCheck> _logger;

		public HealthCheck(ILogger<HealthCheck> logger) {
			_logger = logger;
		}

		[HttpGet]
		public object Get() {
			throw new NotImplementedException();
		}
	}
}
