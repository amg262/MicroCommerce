using System.Text;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace Micro.MessageBus;

public class MessageBus : IMessageBus
{
	private const string ConnectionString =
		"Endpoint=sb://microcommerce.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=TMYA03kermuCbx40up/XienyBzEnk3C64+ASbMv6fyA=";

	public async Task PublishMessage(object message, string topicQueueName,
		CancellationToken cancellationToken = default)
	{
		await using var client = new ServiceBusClient(ConnectionString);
		ServiceBusSender sender = client.CreateSender(topicQueueName);
		var json = JsonConvert.SerializeObject(message);

		ServiceBusMessage busMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(json))
		{
			CorrelationId = Guid.NewGuid().ToString()
		};
		await sender.SendMessageAsync(busMessage, cancellationToken);
		await client.DisposeAsync();
	}
}