using RabbitMQ.Client;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal interface IChannelWrapper {
		Task<PublishStatus> BasicPublish(PublishItem deliveryInfo);
		void Close();
		void UnlockChannel();
		void LockChannel();
	}
}