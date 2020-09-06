using System;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace RmqLib {
    /// <summary>
    /// Интерфейс реализуемый всеми командами приложения подписанными на очередь в rabbitMQ
    /// для которых предусмотрен ответ.
    /// </summary>
    public interface IRmqCommandHandler : IRmqHandler {
        /// <summary>
        /// Имя топика в rabbitMQ
        /// </summary>
        string Topic { get; }
        /// <summary>
        /// Метод выполняемый при входящем сообщении полученном из очереди по топику Topic 
        /// </summary>
        Task<MessageProcessResult> Execute(RequestContext message);
    }
}