<p align="center">
  <image src="https://github.com/milovidov983/BotKeeper/blob/master/logo_full.png" alt="RmqMiniLib logo" width="1200px">
</p>

### RmqMiniLib это dotnet библиотека для удобной работы с RabbitMQ написанная поверх официальной библиотеки


RmqMiniLib делает работу с шиной RabbitMQ более простой, это достигается за счет того что при вызове RPC команд или публикации собщенний от программиста убраны всевозможные инфраструктурные и технические аспекты, которые в большинстве случаев не имеют большого значения.



### Первый пример
#### Создание подключения к rmq

Для использования библиотеки необходимо создать экземпляр класса `RabbitHub` в качестве параметра конструктора он приимает объект со всеми необходимыми настройками соединения с rabbitMq:

Пример настроек соединения файл settings.json:

```json
{
    "RmqConfig": {
        "AppId": "ClientExampleService-svc",
        "HostName": "localhost",
        "Password": "guest",
        "Exchange": "my_exchange",
        "UserName": "guest",
        "Queue": "q_testQueue"
    }
}
```

Ниже описан сам процесс получения настроек из файла и создание экземпляра класса RabbitHub который и отвечает за все взаимодействия вашего приложения и шины:


```csharp
	class Startup {
		private RmqConfig rmqConfigInstance;

		public IRabbitHub CreateHub() {
			var builder = new ConfigurationBuilder()
					.AddJsonFile("settings.json", true, true);

			var configuration = builder.Build();
			rmqConfigInstance = new RmqConfig();
			var settingsSection = configuration.GetSection(nameof(RmqConfig));
			settingsSection.Bind(rmqConfigInstance);


			return new RabbitHub(rmqConfigInstance);
		}
	}


```




#### Публикация сообщения всем кто подписан на topic

<p align="center">
  <image src="https://github.com/milovidov983/BotKeeper/blob/master/chrome_mJUHcnFvrn.png" alt="RmqMiniLib basic publish" width="800px">
</p>

Из этого примера видно что 
