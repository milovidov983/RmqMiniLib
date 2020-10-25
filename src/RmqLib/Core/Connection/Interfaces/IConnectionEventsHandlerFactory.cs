namespace RmqLib.Core {
	internal interface IConnectionEventsHandlerFactory {
		IConnectionEventHandlers CreateHandler();
	}
}