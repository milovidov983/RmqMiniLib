<p align="center">
  <image src="https://github.com/milovidov983/BotKeeper/blob/master/logo_full.png" alt="RmqMiniLib logo" width="1200px">
</p>

### RmqMiniLib это dotnet библиотека для удобной работы с RabbitMQ написанная поверх официальной библиотеки


RmqMiniLib делает работу с шиной RabbitMQ более простой, это достигается за счет того что при вызове RPC команд или публикации собщенний от программиста убраны всевозможные инфраструктурные и технические аспекты, которые в большинстве случаев не имеют большого значения.




### Быстрый старт
#### Создание подключения и настройка конфигурации


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

// Стандартные дейсвтия для получения настроек из settings.json
var builder = new ConfigurationBuilder()
		.AddJsonFile("settings.json", true, true);

var configuration = builder.Build();
var rmqConfigInstance = new RmqConfig();
var settingsSection = configuration.GetSection(nameof(RmqConfig));
settingsSection.Bind(rmqConfigInstance);

// Создание экземпляра класса для работы с шиной
IRabbitHub hub = new RabbitHub(rmqConfigInstance);


```

Если все сконфигурировано корректно то после запуска приложения библиотека сама должан подключится к шине, о чем она напишет в консоли, после этого ее можно использовать для публикации сообщений и вызовов RPC




### Пример публикации
#### Публикация сообщения всем кто подписан на topic "broadcastCommand.serverExample.none"


<p align="center">
  <image src="https://github.com/milovidov983/BotKeeper/blob/master/chrome_mJUHcnFvrn.png" alt="RmqMiniLib basic publish" width="800px">
</p>


Создадим класс который мы хотим отправить 

```csharp
class Message {
	public string Body { get; set; }
}
```


Отправка данных осуществляется с помощью метода `PublishAsync` класса `RabbitHub`, метод имеет следующую сигнатуру:
```csharp
Task PublishAsync<TRequest>(string topic, TRequest request, TimeSpan? timeout = null);
```

`topic` - это ключ маршрутизации на который подписан сервис которому адресовано сообщение

`request` - тело сообщения


Пример полностью:


```csharp

IRabbitHub hub = new RabbitHub(rmqConfigInstance);


await hub.PublishAsync("broadcastCommand.serverExample.none",
	new Message {
		Body = "Hello world!"
	}
);
```



***


### RPC вызовы
#### Выполнение команд запрос/ответ с посредником rabbitMq


Для выполнения RPC вызывов используется библиотченый метод
```csharp
Task<TResponse> ExecuteRpcAsync<TResponse, TRequest>(string topic, TRequest request, TimeSpan? timeout = null) 
where TResponse : class;
```


Что бы разобрать пример с RPC вызовом допустим у клиента и сервера к которому мы хотим послать запрос есть общий класс с описанием класса запроса и класса ответа а так же с названием топика по которому будет сделана публикация в rabbitMq. Опишем клаcс подобным образом:

```csharp

/// <summary>
/// Класс "контракт" описывающий данные участвующие в клиент/серверном взаимодействии
/// </summary>
public class ExampleCommand {
	/// <summary>
	/// topic или по другому routingKey на который подписан сервер ожидающий наше сообщение
	/// </summary>
	public const string Topic = "exampleCommand.serverExample.rpc";

	public class Request {
		/// <summary>
		/// Сообщение от клиента
		/// </summary>
		public string Message { get; set; }
	}

	public class Response {
		/// <summary>
		/// Ответ от сервера
		/// </summary>
		public string Message { get; set; }
	}
}

```

Теперь имея на руках такой контракт который поддерживают обе стороны можно переходить к самому вызову:

```csharp

// Пример RPC вызова

var response = await hub.ExecuteRpcAsync<ExampleCommand.Response, ExampleCommand.Request>(
	ExampleCommand.Topic,
	new ExampleCommand.Request {
		Message = "Hello RPC!"
	}
);


Console.WriteLine($"Получен ответ от микросервиса: {response.Message}");
```

***

### Создание подписок на топики RabbitMq

Пока нет возможности полностью описать процесс подписок, но я думаю можно без труда разобратся посмотрев на пример:

https://github.com/milovidov983/RmqMiniLib/blob/master/examples/ExampleServer/ServerExample.Service/Infrastructure/MessageQueueProcessor.cs


Пример создания микросервиса на основе этой библиотеки находится тут:

https://github.com/milovidov983/RmqMiniLib/tree/master/examples/ExampleServer

