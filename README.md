# ThreadMessaging
Dotnet C# Thread Messaging class

I tried to create a ultra simple inter-thread messaging engine.

- Create a MessagingService instance.
- Subscribe or Unsubscribe MessageReceiver with group name
- Call Publish

````
public interface MessageReceiver
{
    Task NewMessageAsync(Message message);
}
````

Look at test code for example.

