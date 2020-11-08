using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib {
    /// <summary>
    /// TODO comment
    /// </summary>
    public class RmqConfig {
        /// <summary>
        /// TODO comment
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// TODO comment
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// TODO comment
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// TODO comment
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// TODO comment
        /// </summary>
        public string Exchange { get; set; }
        /// <summary>
        /// Имя очереди
        /// </summary>
        public string Queue { get; set; }
        /// <summary>
        /// Тайм аут для запроса
        /// </summary>
        public TimeSpan RequestTimeout { get; set; } = new TimeSpan(0, 0, 0, 20);
        /// <summary>
        /// PrefetchCount
        /// </summary>
        public ushort PrefetchCount { get; set; } = 32;
        /// <summary>
        /// Вызывать исключение если количество зарегистрированных обработчиков 
        /// меньше чем количество классов наследников IRabbitCommand
        /// </summary>
        public bool ControlHandlersNumber { get; set; } = true;




        internal void Validate() {
            if (string.IsNullOrWhiteSpace(HostName)) {
                ThrowArgumentException(nameof(HostName));
            }

            if (string.IsNullOrWhiteSpace(UserName)) {
                ThrowArgumentException(nameof(UserName));
            }

            if (string.IsNullOrWhiteSpace(Password)) {
                ThrowArgumentException(nameof(Password));
            }

            if (string.IsNullOrWhiteSpace(Exchange)) {
                ThrowArgumentException(nameof(Exchange));
            }

        }

        private void ThrowArgumentException(string paramName) {
            throw new ArgumentException($"Incorrect RMQ configuration. Configuration parameter \"{paramName}\" is null or empty!");
        }
    }
}
