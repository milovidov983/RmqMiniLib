using System;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace RmqLib {
    /// <summary>
    /// Интерфейс реализуемый методами микросервиса которые не имеют ответа
    /// </summary>
    public interface IRmqNotificationHandler : IRmqHandler {
        /// <summary>
        /// Имя топика в rabbitMQ
        /// </summary>
        string Topic { get; }
        /// <summary>
        /// Метод выполняемый при входящем сообщении полученном из очереди по топику Topic 
        /// </summary>
        Task Execute(DeliveredMessage message);
    }
}