namespace RmqLib.Core {
	internal interface IConsumerEventHandlersFactory {
		IConsumerRegisterEventHandler CreateRegisterEventHandler();
		IConsumerReceiveEventHandelr CreateReceiveEventHandelr();
		IConsumerCommonEventHandelr CreateCommonEventHandelr();
	}
}