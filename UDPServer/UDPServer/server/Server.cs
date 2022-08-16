using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Diagnostics;

public class Server : IDisposable
{
    public UdpClient udpHandler;
    public ByteState state;
    public AsyncCallback recv = null;

    public IPEndPoint? remoteEndPoint;
    public ServerStatus status;

    public bool running = false;

    /* Threads */
    Thread? serverPacketHandlerThread = null;
    Thread? commandExecutorThread = null;

    public Dictionary<IPEndPoint, PlayerProfile> players = new Dictionary<IPEndPoint, PlayerProfile>();
    public Dictionary<IPEndPoint, string> bannedPlayers = new Dictionary<IPEndPoint, string>();
    public Dictionary<byte, Packet> packets = new Dictionary<byte, Packet>();

    private Logger logger;

    public string ip;
    public short port;

    public Server(string ip, short port)
    {
        Thread.CurrentThread.Name = "Main Thread";

        logger = new Logger();
        logger.Log("Initalized Logger", Thread.CurrentThread);

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        logger.Log("Starting Server", Thread.CurrentThread);

        state = new ByteState();
        logger.Log("Byte Organizer Loaded", Thread.CurrentThread);

        udpHandler = new UdpClient();
        logger.Log("Started UDP Service", Thread.CurrentThread);

        this.ip = ip;

        logger.Log(string.Format("Bind To {0}", ip + ":" + port), Thread.CurrentThread);
        this.port = port;

        BindTo(ip, port);

        running = true;
        status = ServerStatus.OK;

        logger.Log("Starting Packet Listener Thread", Thread.CurrentThread);
        serverPacketHandlerThread = new Thread(() => ListenForPackets());
        serverPacketHandlerThread.Name = "Server Thread";
        serverPacketHandlerThread.Start();

        logger.Log("Starting Command Executor Thread", Thread.CurrentThread);
        commandExecutorThread = new Thread(() => ListenForCommands());
        commandExecutorThread.Name = "Command Executor Thread";
        commandExecutorThread.Start();

        stopwatch.Stop();
        logger.Log("Finished in " + stopwatch.ElapsedMilliseconds + "ms", Thread.CurrentThread);
    }

    /// <summary>
    ///  Binds The UdpClient To A Specific IP And Port.
    /// </summary>
    public void BindTo(string ip, short port)
    {
        udpHandler.Client.Bind(new IPEndPoint(IPAddress.Parse(ip), (int)port));
        logger.Log("Initalized Datagram Socket", Thread.CurrentThread);
    }

    /// <summary>
    ///  Begins To Listen For Connections.
    /// </summary>
    public void ListenForPackets()
    {
        logger.Log("Listening For Connections On Port " + port, Thread.CurrentThread);

        if(running)
        {
            udpHandler.BeginReceive(recv = (ar) =>
            {
                try
                {
                    ByteState? so = ar.AsyncState as ByteState;
                    byte[] bytes = udpHandler.EndReceive(ar, ref remoteEndPoint);
                    udpHandler.BeginReceive(recv, so);

                    string data = ReadPacket(bytes);
                    PacketID? id = PacketUtility.IdentifyPacket(data);

                    if (id == PacketID.Login) //Handshake And Login Packet
                    {
                        AcceptConnection(data);
                        return;
                    }

                    if (id == PacketID.Disconnect) //Disconnect Packet
                    {
                        DisconnectPlayer(remoteEndPoint, "Disconnect Requested From Client");
                        return;
                    }

                    if (id == PacketID.UpdatePosition) //Position Update Packett
                    {
                        SendPacketToAllClients(ProcessPositionPacket(data));
                    }

                    if(id == PacketID.Ping) // Ping Request Packet
                    {
                        //logger.Log(string.Format("Recieved Ping Request From {0}" ,remoteEndPoint.ToString()), Thread.CurrentThread);
                        SendPacket(new C07PongPacket(this));
                    }

                }catch(Exception e)
                {

                }
            }, state);
        }
    }
    
    /// <summary>
    /// Listens For Commands.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void ListenForCommands()
    {
        //TODO: Make More Commands, And Make Command Loop
        CommandExecutor executor = new CommandExecutor();
        Command command = executor.ListenForCommand();

        executor.Execute(command, this);
    }

    /// <summary>
    ///  Disconnects A Player.
    /// </summary>
    public void DisconnectPlayer(IPEndPoint remoteEP, string reason)
    {
        PlayerProfile profileOfDisconnectingPlayer = null;

        if (players.ContainsKey(remoteEP))
        {
            profileOfDisconnectingPlayer = players[remoteEP];
        } else
        {
            throw new AccessViolationException("Player Does Not Exist In The Dictionary!");
        }

        logger.Log("Disconnecting Player " + profileOfDisconnectingPlayer.name, Thread.CurrentThread);

        players.Remove(remoteEP);
        SendPacket(new C04DisconnectPlayerPacket(this, reason));

        logger.Log(profileOfDisconnectingPlayer.name + " Disconnected Due To " + reason, Thread.CurrentThread);
    }

    /// <summary>
    ///  Kicks A Player.
    /// </summary>
    public void Kick(IPEndPoint remoteEP, string reason)
    {
        PlayerProfile profileOfDisconnectingPlayer = null;

        if (players.ContainsKey(remoteEP))
        {
            profileOfDisconnectingPlayer = players[remoteEP];
        }
        else
        {
            throw new AccessViolationException("Player Does Not Exist In The Dictionary!");
        }

        logger.Log("Kicking Player " + profileOfDisconnectingPlayer.name, Thread.CurrentThread);

        players.Remove(remoteEP);
        SendPacket(new C04DisconnectPlayerPacket(this, reason));

        logger.Log(profileOfDisconnectingPlayer.name + " Kicked Due To " + reason, Thread.CurrentThread);
    }

    /// <summary>
    ///  Bans A Player.
    /// </summary>
    public void Ban(IPEndPoint remoteEP, string reason)
    {
        PlayerProfile profileOfDisconnectingPlayer = null;

        if (players.ContainsKey(remoteEP))
        {
            profileOfDisconnectingPlayer = players[remoteEP];
        }
        else
        {
            throw new AccessViolationException("Player Does Not Exist In The Dictionary!");
        }

        logger.Log("Kicking Player " + profileOfDisconnectingPlayer.name, Thread.CurrentThread);

        bannedPlayers.Add(remoteEP, reason);
        players.Remove(remoteEP);

        SendPacket(new C04DisconnectPlayerPacket(this, reason));

        logger.Log(profileOfDisconnectingPlayer.name + " Banned Due To " + reason, Thread.CurrentThread);
    }

    /// <summary>
    ///  Accepts An Incoming UDP Connection.
    /// </summary>
    public void AcceptConnection(string playerData)
    {
        MD5 hash = MD5.Create();
        logger.Log("Incoming Connection From " + remoteEndPoint?.ToString(), Thread.CurrentThread);
        try
        {
            Property property = new Property("playerData");

            foreach (var str in playerData.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
                property.AddProperty(str);

            if (ContainsInvalidCharacters(property.properties[2]))
            {
                SendPacket(new C02FailedLoginPacket(this, "Your Player Name Contains Invalid Characters!"));
                return;
            }

            byte[] hashBytes = hash.ComputeHash(Encoding.UTF8.GetBytes(property.getProperties()[2]));

            PlayerProfile profile = PlayerProfile.fromProperties(property);

            foreach (PlayerProfile p in players.Values)
            {
                if (p.name == profile.name)
                {
                    SendPacket(new C02FailedLoginPacket(this, "A Player With The Same Name Is Already Logged In!"));
                    return;
                }
            }

            foreach (IPEndPoint ep in bannedPlayers.Keys)
            {
                if (ep.Address.ToString() == remoteEndPoint.Address.ToString())
                {
                    SendPacket(new C02FailedLoginPacket(this, bannedPlayers[ep]));
                    return;
                }
            }

            players.Add(remoteEndPoint, profile);

            if (property.getProperties()[1] != toStringFromHash(hashBytes))
            {
                SendPacket(new C02FailedLoginPacket(this, "MD5 Hash Exchange Invalid!"));
                players.Remove(remoteEndPoint);
                logger.Log("MD5 Hash Encryption Handshake Failed: Hash Difference:" + "\n" + "Hash Generated: " + toStringFromHash(hashBytes) + "\n" + "Hash From Packet: " + property.getProperties()[0] + "\n" + new CryptographicException().StackTrace, Thread.CurrentThread);
                return;
            }

            logger.Log("Encryption Handshake Passed With MD5 Signature " + toStringFromHash(hashBytes), Thread.CurrentThread);
            logger.Log("GUID For Connecting Player Is " + profile.playerGuid.ToString(), Thread.CurrentThread);
            C03LoginSuccessPacket entityDataPacket = (C03LoginSuccessPacket)SendPacket(new C03LoginSuccessPacket(this, 0, 0, 0));
            SendPacketToAllClients(new C06InstantiatePacket(this, profile.name));

            logger.Log("Player Name Is " + property.properties[2], Thread.CurrentThread);
            logger.Log(string.Format(remoteEndPoint?.ToString() + " Logged In At Position {0},{1},{2} With Entity ID {3}", new Object[]
            {
                    entityDataPacket.xStart,
                    entityDataPacket.yStart,
                    entityDataPacket.zStart,
                    entityDataPacket.id
            }), Thread.CurrentThread);
        }
        catch (Exception e)
        {
            SendPacket(new C02FailedLoginPacket(this, "Failed Parsing Data"));
            logger.Log("Player Data Invalid" + "\n ------- Message ------- \n" + e.Message + "\n ------- Stack Trace ------- \n" + e.StackTrace, Thread.CurrentThread);
        }
    }

    private bool ContainsInvalidCharacters(string name)
    {
        foreach(char c in new string(":;~`\n+=_'><,|| "))
        {
            if(name.Contains(c))
            {
                return true;
            }
        }

        return false;
    }

    public Packet ProcessPositionPacket(string data)
    {
        Property positionProperty = new Property("playerPosition");

        foreach (var str in data.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
            positionProperty.AddProperty(str);

        Vector pos = new Vector(float.Parse(positionProperty.properties[1]), float.Parse(positionProperty.properties[2]), float.Parse(positionProperty.properties[3]));
        return new C05UpdatePositionPacket(this, pos, positionProperty.properties[4]);
    }

    /// <summary>
    ///  Sends A Packet.
    /// </summary>
    /// <param name="packet">Packet Class To Be Sent.</param>
    /// <returns>The Packet That Was Sent.</returns>
    public Packet SendPacket(Packet? packet)
    {
        udpHandler.Send(packet.data, packet.data.Length, remoteEndPoint);
        return packet;
    }

    public Packet SendPacketTo(Packet? packet, IPEndPoint ep)
    {
        udpHandler.Send(packet.data, packet.data.Length, ep);
        return packet;
    }

    public Packet SendPacketToAllClients(Packet? packet)
    {
        foreach(IPEndPoint remoteEP in players.Keys)
        {
            udpHandler.Send(packet.data, packet.data.Length, remoteEP);
        }

        return packet;
    }

    /// <summary>
    ///  Reads A Packet Sent.
    /// </summary>
    /// <param name="data">Data In The Packet.</param>
    /// <returns>String Decoded From The Packet Data.</returns>
    public string ReadPacket(byte[] data)
    {
        return Encoding.UTF8.GetString(data);
    }

    /// <summary>
    ///  Closes The Server And Releases Any Resources.
    /// </summary>
    public void CloseServer()
    {
        logger.Log("Closing Server", Thread.CurrentThread);
        running = false;

        GC.SuppressFinalize(this);
        SendPacketToAllClients(new C04DisconnectPlayerPacket(this, "Server Closed"));

        logger.Log("Clearing Player Data", Thread.CurrentThread);
        players.Clear();

        logger.Log("Closing UDP Sockets", Thread.CurrentThread);
        udpHandler.Close();

        logger.Log("Saving Log", Thread.CurrentThread);
        logger.SaveLog("log.log");

        Console.WriteLine("Server Stopped. Press Any Key To Continue...");

        Console.ReadKey();
    }

    /// <summary>
    ///  Converts A Hash To A Readable String.
    /// </summary>
    /// <param name="hash">Byte Array Respective To The Hash.</param>
    /// <returns>String Respective To The Hash.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public string toStringFromHash(byte[] hash)
    {
        StringBuilder sb = new StringBuilder();

        for(int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("x2"));
        }

        return sb.ToString();
    }

    public void Dispose()
    {
        CloseServer();
        GC.SuppressFinalize(this);
    }
}
