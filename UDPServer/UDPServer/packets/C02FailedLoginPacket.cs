using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class C02FailedLoginPacket : Packet
{
    string message;

    public C02FailedLoginPacket(Server _server, string _message) : base(_server)
    {
        message = _message;
        Console.WriteLine("Connecting Player Was Disconnected: " + message);
        base.data = Encoding.UTF8.GetBytes(message);
    }

    public override void DecodeData()
    {
        
    }

    public override void EncodeData()
    {

    }
}
