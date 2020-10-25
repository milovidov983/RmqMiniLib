namespace RmqLib.Core {
	internal interface IConsumerEventHandlersFactory {
		IConsumerEventHandlers CreateHandler();
	}
}