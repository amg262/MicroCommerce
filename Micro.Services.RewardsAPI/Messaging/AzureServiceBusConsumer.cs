using System.Text;
using Azure.Messaging.ServiceBus;
using Micro.Services.RewardsAPI.Message;
using Micro.Services.RewardsAPI.Service;
using Newtonsoft.Json;

namespace Micro.Services.RewardsAPI.Messaging;

/// <summary>
/// Consumes messages from Azure Service Bus and processes them.
/// </summary>
public class AzureServiceBusConsumer : IAzureServiceBusConsumer
{
	private readonly string serviceBusConnectionString;
	private readonly string orderCreatedTopic;
	private readonly string orderCreatedRewardSubscription;
	private readonly IConfiguration _configuration;
	private readonly RewardService _rewardService;

	private ServiceBusProcessor _rewardProcessor;

	/// <summary>
	/// Initializes a new instance of the <see cref="AzureServiceBusConsumer"/> class.
	/// </summary>
	/// <param name="configuration">Configuration for accessing application settings.</param>
	/// <param name="rewardService">Service to handle reward logic.</param>
	public AzureServiceBusConsumer(IConfiguration configuration, RewardService rewardService)
	{
		_rewardService = rewardService;
		_configuration = configuration;

		// Retrieve connection string and topic/subscription names from configuration
		serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
		orderCreatedTopic = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
		orderCreatedRewardSubscription =
			_configuration.GetValue<string>("TopicAndQueueNames:OrderCreated_Rewards_Subscription");

		var client = new ServiceBusClient(serviceBusConnectionString);
		_rewardProcessor = client.CreateProcessor(orderCreatedTopic, orderCreatedRewardSubscription);
	}

	/// <summary>
	/// Starts processing messages from the service bus.
	/// </summary>
	public async Task Start()
	{
		_rewardProcessor.ProcessMessageAsync += OnNewOrderRewardsRequestReceived;
		_rewardProcessor.ProcessErrorAsync += ErrorHandler;
		await _rewardProcessor.StartProcessingAsync();
	}

	/// <summary>
	/// Stops processing messages and disposes the processor.
	/// </summary>
	public async Task Stop()
	{
		await _rewardProcessor.StopProcessingAsync();
		await _rewardProcessor.DisposeAsync();
	}

	/// <summary>
	/// Handles the event when a new reward message is received.
	/// </summary>
	/// <param name="args">Event arguments containing the service bus message.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	private async Task OnNewOrderRewardsRequestReceived(ProcessMessageEventArgs args)
	{
		//this is where you will receive message
		var message = args.Message;
		var body = Encoding.UTF8.GetString(message.Body);

		RewardsMessage objMessage = JsonConvert.DeserializeObject<RewardsMessage>(body);
		try
		{
			//TODO - try to log email
			await _rewardService.UpdateRewards(objMessage);
			await args.CompleteMessageAsync(args.Message);
		}
		catch (Exception ex)
		{
			throw;
		}
	}

	private Task ErrorHandler(ProcessErrorEventArgs args)
	{
		Console.WriteLine(args.Exception.ToString());
		return Task.CompletedTask;
	}
}