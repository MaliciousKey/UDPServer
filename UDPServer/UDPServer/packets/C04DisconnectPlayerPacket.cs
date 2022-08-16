using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class C04DisconnectPlayerPacket : Packet
{
    private string reason;

    public C04DisconnectPlayerPacket(Server server, string _reason) : base(server)
    {
        this.reason = "DisconnectRequest:" + _reason;
        base.data = Encoding.UTF8.GetBytes(reason);
    }

    public override void DecodeData()
    {
        throw new NotImplementedException();
    }

    public override void EncodeData()
    {
        
    }
}