using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib.Core {
	class ChannelGuardService {
		private readonly IChannelWrapper channel;
		private readonly IConnectionWrapper connection;

		public ChannelGuardService(IChannelPool channelPool, IConnectionWrapper connection) {
			channel = channelPool.GetChannel();
			this.connection = connection;

			connection.BindEventHandlers(c => c.ConnectionShutdown += ConnectionShutdown);
			connection.RegisterUnsubscribeAction(c => c.ConnectionShutdown -= ConnectionShutdown);
		}


		public Task ConsumerRegistred(object sender, ConsumerEventArgs @event) {
			// судя по всему это тут надо для того что 
			// бы consumer успевал зарегистрироваться при потере связи
			channel.UnlockChannel();


			Console.WriteLine($"ConsumerEvent ConsumerRegistred. Channel unlocked");
			return Task.CompletedTask;
		}



		private void ConnectionShutdown(object sender, ShutdownEventArgs e) {
			// судя по всему это тут надо для того что 
			// бы consumer успевал зарегистрироваться при потере связи
			channel.LockChannel();


			Console.WriteLine($"ConnectionEvent ConnectionShutdown. Channel is locked. ReplyText: {e.ReplyText}");
		}
	}
}
