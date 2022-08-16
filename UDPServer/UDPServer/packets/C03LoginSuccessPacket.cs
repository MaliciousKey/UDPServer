using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class C03LoginSuccessPacket : Packet
{
    public int xStart;
    public int yStart;
    public int zStart;

    public new int id;

    public C03LoginSuccessPacket(Server server, int v1, int v2, int v3) : base(server)
    {
        int _id = new Random().Next(int.MinValue, int.MaxValue);

        xStart = v1;
        yStart = v2;
        zStart = v3;
        id = _id;

        string data = string.Format("X:{0} Y:{1} Z:{2} ID:{3}", new object?[]
        {
            v1,
            v2,
            v3,
            _id
        });
        base.data = Encoding.UTF8.GetBytes(data);
    }

    public override void DecodeData()
    {
        throw new NotImplementedException();
    }

    public override void EncodeData()
    {
        
    }
}