using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib2 {

	internal class ConnectionWrapper {
		private RabbitMQ.Client.IConnection connection;
		private readonly IConnectionFactory factory;
		private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

		public ConnectionWrapper(IConnectionFactory factory) {
			this.factory = factory;
		}

		public async Task<RabbitMQ.Client.IModel> CreateChanelModel() {
			try {
				await semaphore.WaitAsync();
				return connection.CreateModel();
			} finally {
				semaphore.Release();
			}
		}


		public async Task CreateRmqConnection() {
			try {
				await semaphore.WaitAsync();
				connection = factory.CreateConnection();
			} finally {
				semaphore.Release();
			}
			//InitChanels().GetAwaiter().GetResult();
		}
	}


	internal class ConnectionManager : IConnectionManager {
		private readonly RmqConfig config;
		private readonly ILogger logger;
		private ChanelFactory chanelFactory;
		private ConnectionWrapper connection;
		private IConnectionFactory factory;

		public IConnection RmqConnection { get; private set; }

		public ConnectionManager(RmqConfig config, ILogger logger) {
			this.config = config;
			this.logger = logger;

			factory = InitConnectionFactory();
			connection = new ConnectionWrapper(factory);
			StartConnection();
			chanelFactory = new ChanelFactory(this, config.Exchange);
			InitChanels().GetAwaiter().GetResult();
		}


		/// <summary>
		/// TODO comment
		/// </summary>
		private void StartConnection() {
			try {
				connection.CreateRmqConnection().GetAwaiter().GetResult();
			} catch (Exception e) {
				var message = $"Failed connect to the RabbitMQ: {e.Message} ";
				logger?.LogError(message);
				throw;	
			}
		}

		private IConnectionFactory InitConnectionFactory() {
			var factory = new RabbitMQ.Client.ConnectionFactory {
				HostName = config.HostName,
				Password = config.Password,
				UserName = config.UserName
			};
			factory.DispatchConsumersAsync = true;
			factory.AutomaticRecoveryEnabled = true;
			return factory;
		}


		private async Task InitChanels() {
			var rmqChanel = await chanelFactory.CreateChanel();
			var abstractionChanel = chanelFactory.GetOutChannel();
			abstractionChanel.InitChanel(rmqChanel);
		}

		public void Reconnect() {
			StartConnection();
			InitChanels().GetAwaiter().GetResult();
		}
	}
}
