using RabbitMQ.Client;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal interface IChannelWrapper {
		Task<PublishStatus> BasicPublish(DeliveryInfo deliveryInfo);
		void Close();
		void UnlockChannel();
		Task LockChannel();
	}
}