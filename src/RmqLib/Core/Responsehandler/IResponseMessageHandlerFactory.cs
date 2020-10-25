namespace RmqLib {
	interface IResponseMessageHandlerFactory {
		IResponseMessageHandler GetHandler();
	}
}
