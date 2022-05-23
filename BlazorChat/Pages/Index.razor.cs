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
            public override Task NewMessageAsync(Message message, CancellationToken cancel)
            {
                return _index.NewMessageAsync(message, cancel);
            }
        }

        List<string> messagesDisplayList = new List<string>();
        string nextMessage = Guid.NewGuid().ToString();
        MyMessageReceiver receiver;
        [Inject]
        public MessagingService _msgservice { get; set; }
        public Tenant _tenant;
        public Message _template;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                receiver = new MyMessageReceiver(this);
                _tenant = _msgservice.OpenTenant("demo");
                _template = _tenant.NewMessage("chat", "user");
                await _tenant.SubscribeAsync(_template.groupId, receiver);
                StateHasChanged();
            }
        }
        public async Task NewMessageAsync(Message message, CancellationToken cancel)
        {
            if (_tenant.MessageMatchTemplate(message, _template))
            {
                messagesDisplayList.Add(message.data);
                await InvokeAsync(StateHasChanged);
            }
        }
        public Task SendMessage(int type)
        {
            if (type == 0)
                return _tenant.PublishAsync(_tenant.NewMessageFromTemplate(_template, nextMessage));
            else //if (type==1)
                return _tenant.PublishExceptAsync(_tenant.NewMessageFromTemplate(_template, nextMessage), 60, receiver);
        }
        public async ValueTask DisposeAsync()
        {
            if (_tenant != null)
                await _tenant.UnsubscribeAsync(_template.groupId, receiver);
        }
    }
}