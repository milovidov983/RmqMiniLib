using ExampleContracts1;
using RmqLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExampleService1.Commands {
	public class TestCommandOne : IRmqCommandHandler {
		public string Topic => SumExampleCommand.Topic;

		public async Task<ResponseMessage> Execute(DeliveredMessage message) {
			var data = message.GetContent<SumExampleCommand.Request>();


			await Task.Yield();
			return new ResponseMessage().Create(new SumExampleCommand.Response {
				Sum = data.A + data.B
			});
		}
	}
}
