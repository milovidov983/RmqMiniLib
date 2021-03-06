﻿using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib.Core {
	internal class ChannelGuardService {
		private readonly IChannelWrapper channel;
		private readonly IRmqLogger logger;

		public ChannelGuardService(
			IChannelPool channelPool,
			IRmqLogger logger,
			IConnectionEventHandlers connectionEventHandlers,
			IConsumerMainEventHandlers consumerEventHandlers) {

			this.logger = logger;

			channel = channelPool.GetChannelWrapper();

			connectionEventHandlers.AddHandler(ConnectionShutdownEventHandler);
			consumerEventHandlers.AddRegisterHandler(ConsumerRegistredEventHandelr);
		}


		public void ConsumerRegistredEventHandelr(object sender, ConsumerEventArgs @event) {
			logger.Debug($"ConsumerRegistred. Try to unlock channel");

			// судя по всему это тут надо для того что 
			// бы consumer успевал зарегистрироваться при потере связи
			channel?.UnlockChannel();


			logger.Debug($"ConsumerRegistred. Channel unlocked");
			
		}



		private void ConnectionShutdownEventHandler(object sender, ShutdownEventArgs e) {
			logger.Debug($"ConnectionShutdown." +
				$" Try to lock channel. ReplyText: {e.ReplyText}");

			// судя по всему это тут надо для того что 
			// бы consumer успевал зарегистрироваться при потере связи
			channel?.LockChannel();


			logger.Debug($"ConnectionEvent ConnectionShutdown. Channel is locked");
		}
	}
}
