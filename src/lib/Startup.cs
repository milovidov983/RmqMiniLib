using Microsoft.Extensions.DependencyInjection;
using System;

namespace RmqLib {
	public class Startup {
		public static void Init(IServiceCollection services) {
			// 1. create connection
			// 2. create channel
			// 3. create response handler
			// 4. если у сервиса есть подходящие команды то инициализировать их
			// 5. привязать пользовательские команды к топикам
			// 6. добавить как singleton сервис rmqSender
		}
	}
}
