# ThreadMessaging
Dotnet C# Thread Messaging class

I tried to create a ultra simple inter-thread messaging engine.

- Create a MessagingService instance.
- Subscribe or Unsubscribe IMessageReceiver with group name
- Call Publish

````
public interface IMessageReceiver
{
    Task NewMessageAsync(Message message);
}
````

Look at test code for example.

