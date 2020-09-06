# RmqMiniLib

RmqMiniLib

# Тестирование

Запустить RabbitMQ на локальной машине с дефолтными настройками
docker run -d --hostname my-rabbit --name some-rabbit -p 8080:15672 rabbitmq:3-management

Запустить server и client

Выполнить GET запрос для тестирования RPC
http://localhost:9001/Example/ExecuteRpc

Выполнить GET запрос для тестирования Notify
http://localhost:9001/Example/SendNotify
