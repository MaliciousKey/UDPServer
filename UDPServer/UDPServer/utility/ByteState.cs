using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ByteState
{
    public static int bufSize = 8 * 1024;

    public class State
    {
        public byte[] buffer = new byte[bufSize];
    }
}
