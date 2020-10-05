using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib2 {
	public class RpcClient {
		private IDeliveryInfoBuilder deliveryInfoBuilder;
		private IPublisher publisher;


		public async Task<TResponse> GetResponseAsync<TResponse, TRequest>(string topic, TRequest request, TimeSpan? timeout = null) 
			where TRequest : class
			where TResponse : class {

			DeliveryInfo deliveryInfo = deliveryInfoBuilder.CreateDeliveryInfo(topic, request, timeout);
			
			DeliveredMessage deliveredMessage = publisher.Publish(deliveryInfo);

			TResponse resp = await deliveredMessage.GetResponse<TResponse>();

			return resp;
		}
	}

	internal class ReplayTask {

	}

	internal interface IDeliveryInfoBuilder {
		/// <summary>
		/// Сериализация реквеста и другие приготовления
		/// </summary>
		/// <typeparam name="TRequest"></typeparam>
		/// <param name="topic"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		DeliveryInfo CreateDeliveryInfo<TRequest>(string topic, TRequest request, TimeSpan? timeout) where TRequest : class;
	}


}
