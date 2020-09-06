using RmqLib;
using Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SC = Server.Contracts;

namespace ExampleServer.Commands {



	public class GetMessageExampleCommand : CommandContext, IRmqCommandHandler {
		private readonly DatabaseService databaseService;

		public string Topic => SC.GetMessageExample.Topic;

		public GetMessageExampleCommand(DatabaseService databaseService) {
			this.databaseService = databaseService;
		}

		public override async Task ExecuteImpl(RequestContext request) {
			// Получаем данные из сообщения и десериализуем их к релевантному объекту
			var req = request.GetContent<SC.GetMessageExample.Request>();

			// Выполняем бизнес логику команды
			var data = await databaseService.GetData(req.Id);

			// Подготавливаем ответ
			var response = ResponseMessage.Create(
				new SC.GetMessageExample.Response {
					Data = data
				}
			);

			// Отвечаем
			await hub.SetRpcResultAsync(request, response);
		}
	}
}