using Microsoft.Extensions.Logging;
using RmqLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CMCI = Common.Contracts.Infrastructure;
namespace ServerExample.Service.Infrastructure {
	public abstract class BaseMessageHandler : DefaultRabbitCommand {
		protected readonly ErrorHandler errorHandler;
		protected readonly ILogger logger;
		protected readonly Settings settings;

		public BaseMessageHandler(Context context) {
			this.errorHandler = context.ErrorHandler;
			//this.logger = Settings.Logger;
			//this.settings = Settings.Instance;
		}

		protected Task OnError(RequestContext ctx, CMCI.MicroServiceException ex) {
			return errorHandler.OnError(ctx, ex);
		}

		protected abstract Task ExecuteImpl(RequestContext ctx);

		public virtual Task OnCommited(RequestContext ctx) => Task.CompletedTask;
	}
}
