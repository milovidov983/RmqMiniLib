namespace RmqLib.Core {
	internal interface IPublisherFactory {
		IPublisher GetBasicPublisher();
	}
}