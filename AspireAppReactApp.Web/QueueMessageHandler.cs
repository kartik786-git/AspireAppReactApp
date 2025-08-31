using SharedClassLibrary;

namespace AspireAppReactApp.Web
{
    public sealed class QueueMessageHandler
    {
        public event Func<UploadResult, Task>? MessageReceived;

        public Task OnMessageReceivedAsync(UploadResult? result)
        {
            if (result is null)
            {
                return Task.CompletedTask;
            }

            return MessageReceived?.Invoke(result) ?? Task.CompletedTask;
        }
    }
}
