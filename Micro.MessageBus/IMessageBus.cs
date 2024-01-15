namespace Micro.MessageBus;

public interface IMessageBus
{
	Task PublishMessage(object message, string topicQueueName, CancellationToken cancellationToken = default);
}