using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class C06InstantiatePacket : Packet
{
    public C06InstantiatePacket(Server _server, string name) : base(_server)
    {
        base.data = Encoding.UTF8.GetBytes("CreatePlayer:" + name);
    }

    public override void DecodeData()
    {

    }

    public override void EncodeData()
    {

    }
}
