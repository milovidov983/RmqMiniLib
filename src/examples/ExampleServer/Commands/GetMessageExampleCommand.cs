using ExampleContracts1;
using RmqLib;
using Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExampleServer.Commands {

	public abstract class CommandContext {
		protected IRmqSender hub;
		public async Task<MessageProcessResult> Execute(RequestContext request) {
			hub = request.Hub;
			try {
				await ExecuteImpl(request);
				return MessageProcessResult.Ack;
			} catch(RmqException e) {
				//logError
				await LogError(request, e);
			}
			return MessageProcessResult.Reject;

		}

		private async Task LogError(RequestContext request, RmqException e) {
			Console.WriteLine(nameof(CommandContext) + " ERROR! : ");
			Console.WriteLine(e.Message);

			if (request.IsRpcMessage()) {
				await hub.SetRpcErrorAsync(request, e);
			}
		}

		public abstract Task ExecuteImpl(RequestContext message);

		
	}


	public class GetMessageExampleCommand : CommandContext, IRmqCommandHandler {
		private readonly DatabaseService databaseService;

		public string Topic => SumExampleCommand.Topic;

		public GetMessageExampleCommand(DatabaseService databaseService) {
			this.databaseService = databaseService;
		}

		public override async Task ExecuteImpl(RequestContext request) {
			// Получаем данные из сообщения и десериализуем их к релевантному объекту
			var req = request.GetContent<SumExampleCommand.Request>();

			// Выполняем бизнес логику команды
			var data = await databaseService.GetData(req.Id);

			// Подготавливаем ответ
			var response = ResponseMessage.Create(
				new SumExampleCommand.Response {
					Data = data
				}
			);

			// Отвечаем
			await hub.SetRpcResultAsync(request, response);
		}
	}
}