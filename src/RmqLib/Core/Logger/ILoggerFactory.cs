namespace RmqLib.Core.Logger {
	internal interface ILoggerFactory {
		IRmqLogger CreateLogger();
	}
}