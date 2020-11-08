using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RmqLib.Core {
	internal interface IChannelCommand {

	}
	internal class BasicPublishCommand: IChannelCommand {
		public PublishItem Payload { get; set; }
	
		public TaskCompletionSource<PublishStatus> Status { get; set; }
	}


	internal class ChannelWrapper : IChannelWrapper {
		private readonly IModel rmqchannel;
		private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

		public Channel<IChannelCommand> channel { get; set; }

		public IModel GetTrueChannel() {
			return rmqchannel;
		}

		public ChannelWrapper(IModel channel) {
			this.rmqchannel = channel;
			this.channel = Channel.CreateUnbounded<IChannelCommand>();
		}

		public async Task Start() {
			while (await channel.Reader.WaitToReadAsync()) {
				var command = await channel.Reader.ReadAsync();

				switch(command)
				{
					case BasicPublishCommand cmd {
						Handle(cmd);
						break;
					}
					default:
						throw new NotImplementedException("")
				};
			}
		}

		private void Handle(BasicPublishCommand command) {
			var publishItem = command.Payload;

			if (publishItem.IsCanceled) {
				command.Status?.TrySetResult(PublishStatus.SuccessStatus);
				return;
			}
			try {
				var props = rmqchannel.CreateBasicProperties();
				props.CorrelationId = publishItem.CorrelationId;
				props.ReplyTo = publishItem.ReplyTo;
				props.AppId = publishItem.AppId;

				rmqchannel.BasicPublish(
					exchange: publishItem.Exchange,
					routingKey: publishItem.RoutingKey,
					basicProperties: props,
					body: publishItem.Body
					);

				command.Status?.TrySetResult(PublishStatus.SuccessStatus);
				return;
			} catch (Exception ex) {
				command.Status?.TrySetResult(new PublishStatus {
					IsSuccess = false,
					Error = ex
				}) ?? throw new Exception(ex);
				return;
			} 
		}

		public async Task<Task<PublishStatus>> BasicPublish(BasicPublishCommand command) {
			command.Status = new TaskCompletionSource<PublishStatus>();
			await channel.Writer.WriteAsync(command);
			return command.Status.Task;
		}



		public async Task BasicPublish(string exchange, string routingKey, IBasicProperties basicProperties, byte[] body) {
			try {
				await semaphore.WaitAsync();
				rmqchannel.BasicPublish(
					exchange: exchange,
					routingKey: routingKey,
					basicProperties: basicProperties,
					body: body);
			} finally {
				semaphore.Release();
			}
		}

		public async Task BasicAck(ulong deliveryTag, bool multiple) {
			try {
				await semaphore.WaitAsync();
				rmqchannel.BasicAck(deliveryTag, multiple);
			} finally {
				semaphore.Release();
			}
		}		
		
		public async Task BasicReject(ulong deliveryTag, bool requeue) {
			try {
				await semaphore.WaitAsync();
				rmqchannel.BasicReject(deliveryTag, requeue);
			} finally {
				semaphore.Release();
			}
		}

		public async Task QueueBind(string queue, string exchange, string routingKey) {
			try {
				await semaphore.WaitAsync();
				rmqchannel.QueueBind(queue,exchange,routingKey);
			} finally {
				semaphore.Release();
			}
		}




		public async Task BasicConsume(IBasicConsumer consumer, string queue, bool autoAck) {
			try {
				await semaphore.WaitAsync();
				rmqchannel.BasicConsume(
					queue: queue,
					autoAck: autoAck,
					consumer: consumer);
				
			} finally {
				semaphore.Release();
			}
		}

		public async Task<IBasicProperties> CreateBasicProperties() {
			try {
				await semaphore.WaitAsync();
				return rmqchannel.CreateBasicProperties();
			} finally {
				semaphore.Release();
			}
		}


		public void Close() {
			try {
				semaphore.Wait();
				if (!rmqchannel.IsClosed) {
					rmqchannel.Abort();
				}
			} finally {
				semaphore.Release();
			}
		}

		/// <summary>
		/// Вызвать при потере соединения
		/// </summary>
		public void LockChannel() {
			semaphore.Wait();
		}

		/// <summary>
		/// Восстановить доступ к каналу после регистрации consumer
		/// </summary>
		public void UnlockChannel() {
			semaphore.Release();
		}


	}
}
