using RabbitMQ.Client;
using System.Threading.Tasks;

namespace RmqLib.Core {
	internal interface IChannelWrapper {
		Task<PublishStatus> BasicPublish(PublishItem deliveryInfo);
		void Close();
		void UnlockChannel();
		void LockChannel();
		IModel GetDebugChannel();
	}
}