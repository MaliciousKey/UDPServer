using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class C05UpdatePositionPacket : Packet
{
    public C05UpdatePositionPacket(Server _server, Vector position, string playerName) : base(_server)
    {
        base.data = Encoding.UTF8.GetBytes(string.Format("{0}\n{1}\n{2}\n{3}", new object[]
        {
            position.x,
            position.y,
            position.z,
            playerName
        }));
    }

    public override void DecodeData()
    {

    }

    public override void EncodeData()
    {

    }
}
