﻿using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal class ConnectionManager : IConnectionManager {
		private readonly IChannelWrapper channel;

		public ConnectionManager(IChannelWrapper channel) {
			this.channel = channel;
		}

		public Task ConsumerRegistred(object sender, ConsumerEventArgs @event) {
			Console.WriteLine("Rmq connected");
			channel.UnlockChannel();
			return Task.CompletedTask;
		}


		public void ConnectionLostHandler(object sender, ShutdownEventArgs e) {
			Console.WriteLine($"Rmq disconnected {e.Cause}");
			channel.LockChannel().GetAwaiter().GetResult();
		}
	}
}
