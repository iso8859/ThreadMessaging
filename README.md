# ThreadMessaging
Dotnet C# Thread Messaging class

I tried to create a ultra simple inter-thread messaging engine.

I want it to be the simplest multi-tenant thread safe messaging engine.

- Create a MessagingService instance.
- Subscribe or Unsubscribe MessageReceiver with group name
- Call Publish

````
public class _01Simple : MessageReceiver
{
    Message _msgTemplate;
    Tenant _tenant;
    
    // Message loop
    override public Task NewMessageAsync(Message message)
    {
        if (_tenant.MessageMatchTemplate(message, _msgTemplate) && message.data == "hello")
        {
            // Do something with the message
        }
        return Task.CompletedTask;
    }

    public async Task Start()
    {
        MessagingService service = new MessagingService();
        string tenantName = "01simple";
        _tenant = service.OpenTenant(tenantName); // Create or open existing tenant.
        _msgTemplate = _tenant.NewMessage("test", "msg");
        await _tenant.SubscribeAsync(_msgTemplate.groupId, this); // Subscribe to this message queue
        await _tenant.PublishAsync(_tenant.NewMessageFromTemplate(_msgTemplate, "hello")); // Send a message to all tenant where groupId == message templace groupId
        // wait
        await _tenant.UnsubscribeAsync(_msgTemplate.groupId, this);
    }
}
````

You have OnAddedAsync and OnRemovedAsync events to plug this another message solution, for example SignalR.

