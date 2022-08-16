using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class C07PongPacket : Packet
{
    public C07PongPacket(Server _server) : base(_server)
    {

    }

    public override void DecodeData()
    {
        
    }

    public override void EncodeData()
    {
        string pong = "Pong:" + base.server.status.ToString();
        base.data = Encoding.UTF8.GetBytes(pong);
    }
}
