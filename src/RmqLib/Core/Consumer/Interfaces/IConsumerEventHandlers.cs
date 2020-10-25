﻿using RabbitMQ.Client.Events;
using System;

namespace RmqLib.Core {
	internal interface IConsumerEventHandlers {
		void AddHandler(Action<object, ConsumerEventArgs> handler);
	}
}