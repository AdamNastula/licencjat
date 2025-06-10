using System.Net.Sockets;

namespace MultiplayerChessServer;

public class ClientThread
{
    private TcpClient _client;
    private NetworkStream _ownStream;
    private NetworkStream? _enemyStream;
    private bool _white;
    private byte[] _from = new byte[sizeof(UInt64)];
    private byte[] _to = new byte[sizeof(UInt64)];
    private byte[] _piece = new byte[1];
    private byte[] _type = new byte[1];
    private byte[] _additionalInfo = new byte[sizeof(UInt64)];
    
    public ClientThread(TcpClient client, NetworkStream? enemyStream, bool white)
    {
        _client = client;
        _ownStream = _client.GetStream();
        _enemyStream = enemyStream;
        _white = white;
    }

    public async void Start()
    {
        while (_enemyStream is null)
        {
            await Task.Delay(500);
        }

        while (_client.Connected)
        {
            int size = _ownStream.Read(_from, 0, sizeof(UInt64));
            _enemyStream.Write(_from, 0, size);
            size = _ownStream.Read(_to, 0, sizeof(UInt64));
            _enemyStream.Write(_to, 0, size);
            size = _ownStream.Read(_piece, 0, 1);
            _enemyStream.Write(_piece, 0, size);
            size = _ownStream.Read(_type, 0, 1);
            _enemyStream.Write(_type, 0, size);
            size = _ownStream.Read(_additionalInfo, 0, sizeof(UInt64));
            _enemyStream.Write(_additionalInfo, 0, size);
        }
    }

    public void SetEnemyStream(NetworkStream stream)
    {
        _enemyStream = stream;
    }
}