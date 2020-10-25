using System;
using System.Threading.Tasks;

namespace RmqLib {
	public abstract class DefaultRabbitCommand : IRabbitCommand {
		protected IRabbitHub Hub {	get; private set;	}

		public virtual void WithHub(IRabbitHub hub) {
			if (this.Hub != null && this.Hub != hub) {
				throw new InvalidOperationException("Already bound to hub");
			}
			this.Hub = hub;
		}

		public abstract Task<MessageProcessResult> Execute(DeliveredMessage dm);
	}
}
