<p align="center">
  <image src="https://github.com/milovidov983/BotKeeper/blob/master/logo_full.png" alt="RmqMiniLib logo" width="1000px">
</p>

# RmqMiniLib



Обёртка вокруг стандартной библиотеки RabbitMq

## Roadmap

Разработка библиотеки
- написать rpc метод `IRabbitHub.ExecuteRpcAsync<TResponse, TRequest>()` - ok
- протестировать работу `IRabbitHub.ExecuteRpcAsync<TResponse, TRequest>()` - ok
- написать метод `IRabbitHub.PublishAsync<TRequest>()` позволяющий отправлять сообщения без ответа - ok
- протестировать работу `IRabbitHub.PublishAsync<TRequest>()` - ok
- написать функционал подписок - ok
- написать fluet интерфейс для регистрации обработчиков команд - ok
- SetRpcErrorAsync - ok
- протестировать различные сценарии подписок (написать тесты?) -
	-	тест потеря сети - ok

- внедрить зависимость логгера вместо консольлогов
- настройка для отключения вывода логов внутренней работы библиотеки

Шаблон микросервиса
- написать пример микросервиса с использованием новой библиотеки
- протестировать производительность и различные сценарии (написать тесты на интеграционную часть?)

Дополнительно
- очистка очередей
- подумать над концепцией отбрасывания rpc запросов у которых истёк интервал жизни
