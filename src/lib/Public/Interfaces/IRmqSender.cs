using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib {
	public interface IRmqSender {
		Task Notify<TMessage>(string topic, TMessage message) where TMessage : class;
		Task<TResponse> Send<TRequest, TResponse>(string topic, TRequest message, TimeSpan? timeout = null)
			where TRequest : class
			where TResponse : class;
	}
}
