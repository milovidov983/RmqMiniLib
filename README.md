# RmqMiniLib

Обёртка вокруг стандартной библиотеки RabbitMq

## Roadmap

Разработка библиотеки
- написать rpc метод `IRabbitHub.ExecuteRpcAsync<TResponse, TRequest>()` - ok
- протестировать работу `IRabbitHub.ExecuteRpcAsync<TResponse, TRequest>()` - ok
- написать метод `IRabbitHub.PublishAsync<TRequest>()` позволяющий отправлять сообщения без ответа 
- протестировать работу `IRabbitHub.PublishAsync<TRequest>()`
- написать функционал подписок
- протестировать различные сценарии подписок (написать тесты?)
- внедрить зависимость логгера вместо консольлогов

Шаблон микросервиса
- написать пример микросервиса с использованием новой библиотеки
- протестировать производительность и различные сценарии (написать тесты на интеграционную часть?)

Дополнительно
- очистка очередей

# Тестирование

Запустить RabbitMQ на локальной машине с дефолтными настройками
docker run -d --hostname my-rabbit --name some-rabbit -p 8080:15672 rabbitmq:3-management

Запустить server и client

Выполнить GET запрос для тестирования RPC
http://localhost:9001/Example/ExecuteRpc

Выполнить GET запрос для тестирования Notify
http://localhost:9001/Example/SendNotify
