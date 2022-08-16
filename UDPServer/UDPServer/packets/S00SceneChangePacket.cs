using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class S00SceneChangePacket : Packet
{
    public S00SceneChangePacket(Server _server, int sceneID) : base(_server)
    {
        base.data = Encoding.UTF8.GetBytes("ChangeScenePacket:" + sceneID.ToString());
        Console.WriteLine("Requested The Scene Of Index {0} To Be Loaded Throughout All Clients", sceneID);
    }

    public override void DecodeData()
    {
        
    }

    public override void EncodeData()
    {
        
    }
}
