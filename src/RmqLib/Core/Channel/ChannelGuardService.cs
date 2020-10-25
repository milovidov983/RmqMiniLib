using RabbitMQ.Client;
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
			IConsumerEventHandlers consumerEventHandlers) {

			this.logger = logger;

			channel = channelPool.GetChannel();

			connectionEventHandlers.AddHandler(ConnectionShutdownEventHandler);
			consumerEventHandlers.AddHandler(ConsumerRegistredEventHandelr);
		}


		public void ConsumerRegistredEventHandelr(object sender, ConsumerEventArgs @event) {
			logger.Debug($"{nameof(ChannelGuardService)} ConsumerRegistred. Try to unlock channel");

			// судя по всему это тут надо для того что 
			// бы consumer успевал зарегистрироваться при потере связи
			channel?.UnlockChannel();


			logger.Debug($"{nameof(ChannelGuardService)} ConsumerRegistred. Channel unlocked");
			
		}



		private void ConnectionShutdownEventHandler(object sender, ShutdownEventArgs e) {
			logger.Debug($"{nameof(ChannelGuardService)} ConnectionShutdown." +
				$" Try to lock channel. ReplyText: {e.ReplyText}");

			// судя по всему это тут надо для того что 
			// бы consumer успевал зарегистрироваться при потере связи
			channel?.LockChannel();


			logger.Debug($"ConnectionEvent ConnectionShutdown. Channel is locked");
		}
	}
}
