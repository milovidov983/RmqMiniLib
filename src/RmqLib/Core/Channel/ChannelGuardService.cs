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
			IConnectionWrapper connection,
			IRmqLogger logger) {

			this.logger = logger;

			channel = channelPool.GetChannel();
			connection.BindEventHandlers(c => {
				try {
					c.ConnectionShutdown += ConnectionShutdownEventHandler;
					logger.Debug($"{nameof(ChannelGuardService)} binded to connection shutdown event");
				} catch(Exception e) {
					logger.Error($"{nameof(ChannelGuardService)} error to bind to connection shutdown event: {e.Message}");
				}
			});
			connection.RegisterUnsubscribeAction(c => {
				if(this is null) {
					return;
				}
				c.ConnectionShutdown -= ConnectionShutdownEventHandler;
			});
		}


		public Task ConsumerRegistredEventHandelr(object sender, ConsumerEventArgs @event) {
			logger.Debug($"ConsumerEvent ConsumerRegistred. Try to unlock channel");

			// судя по всему это тут надо для того что 
			// бы consumer успевал зарегистрироваться при потере связи
			channel?.UnlockChannel();


			logger.Debug($"ConsumerEvent ConsumerRegistred. Channel unlocked");
			return Task.CompletedTask;
		}



		private void ConnectionShutdownEventHandler(object sender, ShutdownEventArgs e) {
			logger.Debug($"ConnectionEvent ConnectionShutdown. Try to lock channel. ReplyText: {e.ReplyText}");

			// судя по всему это тут надо для того что 
			// бы consumer успевал зарегистрироваться при потере связи
			channel?.LockChannel();


			logger.Debug($"ConnectionEvent ConnectionShutdown. Channel is locked");
		}
	}
}
