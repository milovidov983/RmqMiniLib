using System.Threading.Tasks;

namespace RmqLib {
	public interface IRabbitCommand {
		Task<MessageProcessResult> Execute(DeliveredMessage dm);
		void WithHub(IRabbitHub hub);
	}
}
