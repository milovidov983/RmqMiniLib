namespace RmqLib.Core {
	internal interface ILoggerFactory {
		IRmqLogger CreateLogger();
		IRmqLogger CreateLogger(string prefix);
	}
}