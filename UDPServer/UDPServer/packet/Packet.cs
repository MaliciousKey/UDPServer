using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

public abstract class Packet
{
    public Server server;
    public byte id;
    public byte[]? data;

    public Packet(Server _server) {
        server = _server;
        EncodeData();
    }

    public void SendPacket()
    {
        server.SendPacket(this);
    }

    public abstract void EncodeData();

    public abstract void DecodeData();
}
