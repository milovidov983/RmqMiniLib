using RmqLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CMCI = Common.Contracts.Infrastructure;

namespace ServerExample.Service.Infrastructure {
	public class ErrorHandler {
		private readonly IRabbitHub hub;
		private readonly ILogger logger;

		public ErrorHandler(IRabbitHub hub, ILogger logger) {
			this.hub = hub;
			this.logger = logger;
		}

		public async Task OnError(RequestContext ctx, CMCI.MicroServiceException ex) {
			if (ex.StatusCode == CMCI.StatusCodes.InternalError) {
				logger.Error(ex, ctx.Message.GetRequestAndInnerExceptionInfo(ex));
			} else {
				logger.Warn(ex, CMCI.Extensions.GetRequestInfo(ctx.Message));
			}
			try {
				if (ctx.Message.IsRpcMessage()) {
					await hub.SetRpcErrorAsync(ctx.Message, ex.Message, (int)ex.StatusCode);
				}
			} catch (Exception e) {
				logger.Fatal(e, $"Не удалось отправить сообщение об ошибке вызывающей стороне {ctx.Message.GetAppId()} " +
					$"на вызов {ctx.Message.GetTopic()}, " +
					$"подробности: {e.Message}");
			}
		}
	}
}
