using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib2 {
		public interface IRabbitCommand {
			Task<MessageProcessResult> Execute(DeliveredMessage dm);
			void WithHub(IRabbitHub hub);
		}
	
}
