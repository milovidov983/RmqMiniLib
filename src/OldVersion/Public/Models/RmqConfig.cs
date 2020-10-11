﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public TimeSpan RequestTimeout { get; set; } = new TimeSpan(0, 0, 0, 30);
        /// <summary>
        /// PrefetchCount
        /// </summary>
        public ushort PrefetchCount { get; set; } = 32;
        /// <summary>
        /// Данные для подключения к плагину менеджмента rabbitmq
        /// </summary>
        public ManagamentInfo Managament { get; set; }
        /// <summary>
        /// Очищать топики в очереди к которым нет привязанной команды
        /// </summary>
        public bool IsClearUnusedRoutingKeys { get; set; }


        internal void Validate() {
            if (string.IsNullOrWhiteSpace(HostName)) {
                ThrowArgumentException(nameof(HostName));
            }

            if (string.IsNullOrWhiteSpace(UserName)) {
                ThrowArgumentException(nameof(UserName));
            }

            if (string.IsNullOrWhiteSpace(Password)){
                ThrowArgumentException(nameof(Password));
            }

            if (string.IsNullOrWhiteSpace(Exchange)) {
                ThrowArgumentException(nameof(Exchange));
            }

        }

        private void ThrowArgumentException(string paramName) {
            throw new ArgumentException($"Incorrect RMQ configuration. Configuration parameter \"{paramName}\" is null or empty!");
        }

        public class ManagamentInfo {
            public string ManagmentHostName { get; set; } = "localhost";
			public int Port { get; set; } = 8080;
            public string Login { get; set; } = "guest";
			public string Password { get; set; } = "guest";
        }
    }
}