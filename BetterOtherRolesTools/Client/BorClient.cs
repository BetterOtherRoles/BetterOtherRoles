using System.Net;
using WebsocketsSimple.Client;
using WebsocketsSimple.Client.Models;

namespace BetterOtherRolesTools.Client;

public class BorClient
{
    public readonly MainWindow Window;
    public readonly WebsocketClient Client;
    
    public BorClient(MainWindow window)
    {
        Window = window;
        var config = new ParamsWSClient("127.0.0.1", 3000, false);
        Client = new WebsocketClient(config);
    }

    public async void Start()
    {
        await Client.ConnectAsync();
    }
}