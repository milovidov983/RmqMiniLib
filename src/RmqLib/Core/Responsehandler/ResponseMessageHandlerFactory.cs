using RmqLib.Core;

namespace RmqLib {
	internal class ResponseMessageHandlerFactory : IResponseMessageHandlerFactory {
		private readonly IResponseMessageHandler responseMessageHandler;
	

		public ResponseMessageHandlerFactory(IRmqLogger logger) {
			responseMessageHandler = new ResponseMessageHandler(logger);
		}

		public IResponseMessageHandler GetHandler() {
			return responseMessageHandler;
		}

		public static IResponseMessageHandlerFactory Create(IRmqLogger logger) {
			return new ResponseMessageHandlerFactory(logger);
		}
	}
}
