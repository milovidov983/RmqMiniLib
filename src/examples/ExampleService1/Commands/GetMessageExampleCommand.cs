using ExampleContracts1;
using RmqLib;
using Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExampleServer.Commands {
	public class GetMessageExampleCommand : IRmqCommandHandler {
		private readonly DatabaseService databaseService;

		public string Topic => SumExampleCommand.Topic;

		public GetMessageExampleCommand(DatabaseService databaseService) {
			this.databaseService = databaseService;
		}

		public async Task<ResponseMessage> Execute(DeliveredMessage message) {
			var request = message.GetContent<SumExampleCommand.Request>();

			var data = await databaseService.GetData(request.Id);

			var response = ResponseMessage.Create(
				new SumExampleCommand.Response {
					Data = data
				}
			);

			return response;
		}
	}
}