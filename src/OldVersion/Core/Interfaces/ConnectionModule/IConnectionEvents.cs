namespace RmqLib.Core {
	internal interface IConnectionEvents {
		void BindEventHandlers(IConnectionService connectionService);
	}
}