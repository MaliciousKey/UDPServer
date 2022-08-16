using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

public class PacketUtility
{
    public static PacketID? IdentifyPacket(string data)
    {
        if(data.StartsWith("0x00"))
        {
            return PacketID.Login;
        }

        if(data.StartsWith("0xDC"))
        {
            return PacketID.Disconnect;
        }

        if (data.StartsWith("0xPP"))
        {
            return PacketID.UpdatePosition;
        }

        if(data.StartsWith("0xPI"))
        {
            return PacketID.Ping;
        }

        throw new Exception("Unable To Identify Packet Type! Is The Client Genuine?") as SocketException;
    }
}
