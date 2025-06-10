using System.Net;
using System.Net.Sockets;
using MultiplayerChessServer;

TcpListener mainListener = new TcpListener(Dns.Resolve("localhost").AddressList[0], 10105);
mainListener.Start(20);
List<Thread> clientThreads = new List<Thread>();
Stack<Game> availableGames = new Stack<Game>();
Byte[] message = new Byte[1];

while (true)
{
    TcpClient client = mainListener.AcceptTcpClient();
    NetworkStream clientStream = client.GetStream();
    ClientThread clientThread;
    if (availableGames.Count == 0)
    {
        clientThread = new ClientThread(client, null, true);
        availableGames.Push(new Game(true, clientStream, clientThread));
        clientStream.Write( [1], 0, 1);
    }
    else
    {
        Game game = availableGames.Pop();
        game.GetWaitingClientThread().SetEnemyStream(clientStream);
        clientThread = new ClientThread(client, game.GetWaitingPlayerStream(), false);
        clientStream.Write( [0], 0, 1);
    }
    
    Thread t = new Thread(() => clientThread.Start());
    clientThreads.Add(t);
    t.Start();

    /*foreach (var thread in clientThreads)
    {
        if (!thread.IsAlive)
        {
            thread.Join();
                    
            try
            {
                clientThreads.Remove(thread);
            }
            catch (Exception e) {}
        }
                    
    }*/
}

public class Game
{
    private bool _white;
    private NetworkStream _waitingPlayerStream;
    private ClientThread _waitingClientThread;

    public Game(bool white, NetworkStream waitingPlayerStream, ClientThread waitingClientThread)
    {
        _white = white;
        _waitingPlayerStream = waitingPlayerStream;
        _waitingClientThread = waitingClientThread;
    }

    public bool GetWhite()
    {
        return _white;
    }

    public NetworkStream GetWaitingPlayerStream()
    {
        return _waitingPlayerStream;
    }

    public ClientThread GetWaitingClientThread()
    {
        return _waitingClientThread;
    }
}