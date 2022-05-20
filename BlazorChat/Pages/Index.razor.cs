using Microsoft.AspNetCore.Components;
using ThreadMessaging;

namespace BlazorChat.Pages
{
    public partial class Index : IAsyncDisposable
    {
        public class MyMessageReceiver : MessageReceiver
        {
            Index _index;
            public MyMessageReceiver(Index index)
            {
                _index = index;
            }
            public override Task NewMessageAsync(Message message)
            {
                return _index.NewMessageAsync(message);
            }
        }

        List<string> messages = new List<string>();
        string nextMessage;
        MyMessageReceiver receiver;
        [Inject]
        public MessagingService _msgservice { get; set; }

        public async Task NewMessageAsync(Message message)
        {
            if (message.group == "chat" && message.type == "user")
            {
                messages.Add(message.data);
                await InvokeAsync(StateHasChanged);
            }
        }

        protected override async Task OnInitializedAsync()
        {
            receiver = new MyMessageReceiver(this);
            await _msgservice.SubscribeAsync("chat", receiver);
            base.OnInitialized();
        }

        public Task SendMessage(int type)
        {
            if (type == 0)
                return _msgservice.PublishAsync(new Message("chat", "user", nextMessage));
            else //if (type==1)
                return _msgservice.PublishExceptAsync(new Message("chat", "user", nextMessage), 60, receiver);
        }

        public async ValueTask DisposeAsync()
        {
            await _msgservice.UnsubscribeAsync("chat", receiver);
        }
    }
}