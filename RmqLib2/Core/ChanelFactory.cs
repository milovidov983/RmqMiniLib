using RabbitMQ.Client;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal class ChanelFactory : IChanelFactory {
		private ConnectionManager connectionManager;
		private IRmqChanel outputChanel;
		private readonly string exchangeName;

		public ChanelFactory(ConnectionManager connectionManager, string exchangeName) {
			this.connectionManager = connectionManager;
			this.exchangeName = exchangeName;

			var connection = connectionManager.CreateChanelModel().GetAwaiter().GetResult();
			var chanel = CreateChanel(connection);
			outputChanel = new RmqChanel(connectionManager, chanel);
		}

		public IRmqChanel GetOutChannel() {
			return outputChanel;
		}

		public async Task<IModel> CreateChanel() {
			RabbitMQ.Client.IModel chanel = await connectionManager.CreateChanelModel();

			return CreateChanel(chanel);
		}

		private IModel CreateChanel(IModel chanel) {
			DeclareExchanges(chanel);
			return chanel;
		}

		private void DeclareExchanges(IModel channel) {
			channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);
		}
	}
}