using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib {
	public interface IRabbitHub {
		/// <summary>
		/// Публикация сообщения
		/// </summary>
		/// <typeparam name="TRequest">Тип тела сообщения</typeparam>
		/// <param name="topic">Топик по которому производится публикация</param>
		/// <param name="request">Тело сообщения</param>
		/// <param name="timeout">
		/// Временной промежуток после которого будет вызван 
		/// OperationCanceledException если публикацию не удалось осуществить
		/// </param>
		Task PublishAsync<TRequest>(string topic, TRequest request, TimeSpan? timeout = null) where TRequest : class;

		/// <summary>
		/// Вызов удаленного сервиса
		/// </summary>
		/// <typeparam name="TResponse">Тип ответа</typeparam>
		/// <typeparam name="TRequest">Тип тела сообщения</typeparam>
		/// <param name="topic">Топик по которому производится публикация</param>
		/// <param name="request">Тело сообщения</param>
		/// <param name="timeout">
		/// Временной промежуток после которого будет вызван 
		/// OperationCanceledException если удаленных сервис не ответил
		/// </param>
		Task<TResponse> ExecuteRpcAsync<TResponse, TRequest>(string topic, TRequest request, TimeSpan? timeout = null) 
			where TResponse : class
			where TRequest : class;

		/// <summary>
		/// Ответ на RPC вызов в виде ошибки
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dm">Исходное сообщение RPC запроса</param>
		/// <param name="payload">Текст ошибки</param>
		/// <param name="statusCode">Код ответа опционально</param>
		Task SetRpcErrorAsync(DeliveredMessage dm, string error, int? statusCode = null);

		/// <summary>
		/// Ответ на RPC вызов
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dm">Исходное сообщение RPC запроса</param>
		/// <param name="payload">Тело ответа</param>
		/// <param name="statusCode">Код ответа опционально</param>
		Task SetRpcResultAsync<T>(DeliveredMessage dm, T payload, int? statusCode = null);


	}
}
