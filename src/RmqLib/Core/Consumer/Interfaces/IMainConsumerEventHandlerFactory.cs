namespace RmqLib.Core {
	internal interface IMainConsumerEventHandlerFactory {
		IConsumerMainEventHandlers CreateMainHandler();
	}
}