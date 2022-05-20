using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using BlazorChat;
using BlazorChat.Shared;
using ThreadMessaging;

namespace BlazorChat.Pages
{
    public partial class Index : IMessageReceiver, IAsyncDisposable
    {
        List<string> messages = new List<string>();
        string nextMessage;
        [Inject] 
        public ThreadMessaging.MessagingService _msgservice { get; set; }

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
            await _msgservice.SubscribeAsync("chat", this);
            base.OnInitialized();
        }

        public Task SendMessage()
        {
            return _msgservice.PublishAsync(new Message("chat", "user", nextMessage));
        }

        public async ValueTask DisposeAsync()
        {
            await _msgservice.UnsubscribeAsync("chat", this);
        }
    }
}