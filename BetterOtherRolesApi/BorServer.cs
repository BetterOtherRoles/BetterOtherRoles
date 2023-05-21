using PHS.Networking.Enums;
using PHS.Networking.Server.Enums;
using PHS.Networking.Server.Events.Args;
using WebsocketsSimple.Server;
using WebsocketsSimple.Server.Events.Args;
using WebsocketsSimple.Server.Models;

namespace BetterOtherRolesApi;

public class BorServer
{
    public static readonly BorServer Instance = new(3000);

    public readonly WebsocketServer Server;
    public readonly Dictionary<string, Client> Clients = new();
    public bool Debug = false;

    private BorServer(int port)
    {
        var config = new ParamsWSServer(port);
        Server = new WebsocketServer(config);

        Server.MessageEvent += OnMessage;
        Server.ConnectionEvent += OnConnection;
        Server.ErrorEvent += OnError;
        Server.ServerEvent += OnServerEvent;
    }

    public void Start()
    {
        Server.Start();
        while (true)
        {
        }
    }

    private void OnStart()
    {
        if (Debug) Console.WriteLine(">>[SERVER] started");
    }

    private void OnStop()
    {
        if (Debug) Console.WriteLine(">>[SERVER] stopped");
    }

    private void OnServerEvent(object sender, ServerEventArgs args)
    {
        if (args.ServerEventType == ServerEventType.Start)
        {
            OnStart();
        }
        else if (args.ServerEventType == ServerEventType.Stop)
        {
            OnStop();
        }
    }

    private void OnError(object sender, WSErrorServerEventArgs args)
    {
        if (Debug) Console.WriteLine($">>[ERROR]({args.Connection.ConnectionId}): {args.Message}");
    }

    private void OnConnection(object sender, WSConnectionServerEventArgs args)
    {
        if (args.ConnectionEventType == ConnectionEventType.Connected)
        {
            if (Debug) Console.WriteLine($">>[CONNECTION]({args.Connection.ConnectionId}) Connected");
            Clients[args.Connection.ConnectionId] = new Client(args.Connection);
        }
        else if (args.ConnectionEventType == ConnectionEventType.Disconnect)
        {
            if (Debug) Console.WriteLine($">>[CONNECTION]({args.Connection.ConnectionId}) Disconnected");
            Clients.Remove(args.Connection.ConnectionId);
        }
    }

    private void OnMessage(object sender, WSMessageServerEventArgs args)
    {
        if (args.MessageEventType == MessageEventType.Receive)
        {
            if (Debug) Console.WriteLine($">>[RECEIVE]({args.Connection.ConnectionId}): {args.Message}");
            Clients[args.Connection.ConnectionId].OnMessage(args.Message);
        }
        else if (args.MessageEventType == MessageEventType.Sent)
        {
            if (Debug) Console.WriteLine($">>[SENT]({args.Connection.ConnectionId}): {args.Message}");
        }
    }
}