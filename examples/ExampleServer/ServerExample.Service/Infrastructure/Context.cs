
using RmqLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerExample.Service.Infrastructure {
	public class Context {
		public ErrorHandler ErrorHandler { get; }


		public Context(IRabbitHub hub, ILogger logger) {
			ErrorHandler = new ErrorHandler(hub, logger);
		}

	}
}
