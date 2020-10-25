using RmqLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CMCI = Common.Contracts.Infrastructure;
namespace ServerExample.Service.Infrastructure {
	/// <summary>
	/// Контекст обработчика команды приходящей от шины
	/// Команда для чтения из базы без возможности записи.
	/// </summary>
	public abstract class ReadonlyCommandHandler : BaseMessageHandler {
		public ReadonlyCommandHandler(Context context) : base(context) { }

		public override async Task<MessageProcessResult> Execute(DeliveredMessage dm) {
			var topic = dm.GetTopic();

			var app = dm.GetAppId();


			using var db = new Database();
			//db.ActiveConnection.Open();
			var ctx = new RequestContext(dm, db);
			try {
				await ExecuteImpl(ctx);

				return MessageProcessResult.Ack;
			} catch (CMCI.MicroServiceException ex) {
				await OnError(ctx, ex);
			} catch (Exception ex) {
				await OnError(ctx,
					new CMCI.MicroServiceException($"Message failed: { topic } AppId: {app} Ex: {ex.Message}", ex, CMCI.StatusCodes.InternalError));
			}

			return MessageProcessResult.Reject;
		}
	}
}
