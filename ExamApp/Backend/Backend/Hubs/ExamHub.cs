using Microsoft.AspNetCore.SignalR;

namespace Backend.Hubs
{
    public class ExamHub : Hub
    {
        public Task JoinSession()
        { 
            return Groups.AddToGroupAsync(Context.ConnectionId, "exam");
        }
    }
}


