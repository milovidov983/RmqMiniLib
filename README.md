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
#### Публикация сообщения всем кто подписан на topic


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




### RPC вызовы
#### Выполнение команд запрос/ответ с посредником в виде rabbitMq


Для выполнения RPC вызывов используется библиотченый метод
```csharp
Task<TResponse> ExecuteRpcAsync<TResponse, TRequest>(string topic, TRequest request, TimeSpan? timeout = null) where TResponse : class;
```
