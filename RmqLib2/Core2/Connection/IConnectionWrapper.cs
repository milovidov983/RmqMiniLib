﻿using RabbitMQ.Client;
using RmqLib2.Core2;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal interface IConnectionWrapper {
		IModel CreateChannel();
		void StartConnection();
		bool IsOpen { get; }
		void AddConnectionShutdownHandler(IConnectionManager connectionManager);
	}
}
