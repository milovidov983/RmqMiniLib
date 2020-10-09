using System;
using System.Threading;

namespace RmqLib2.Core2 {
	internal class RecconectionManager : IReconnectionManager {
		private readonly IConnectionWrapper connection;
		private readonly IChannelPool channelPool;
		private readonly Semaphore semaphore = new Semaphore(1, 1);

		public RecconectionManager(IConnectionWrapper connection, IChannelPool channelPool) {
			this.connection = connection;
			this.channelPool = channelPool;
		}

		public void Reconnect() {
			while (!connection.IsOpen) {
				try {
					semaphore.WaitOne();
					if (!connection.IsOpen) {
						connection.StartConnection();
						channelPool.InitChannel(connection.CreateChannel());
					}
				} catch (Exception e) {
					var sleepTimeMs = 3000;
					Log($"Ошибка при попытке переподключится к rmq {e.Message}, след попытка через {sleepTimeMs} ms");
					Thread.Sleep(sleepTimeMs);
				} finally {
					semaphore.Release();
				}
			}
		}


		private void Log(string msg) {
			Console.WriteLine(msg);
		}
	}
}