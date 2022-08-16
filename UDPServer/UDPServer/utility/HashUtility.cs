using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

public class HashUtility
{
    public byte[] hash;
    public MD5 md5;

    public HashUtility(byte[] _hash)
    {
        hash = _hash;
        md5 = MD5.Create();
    }
}
