using System;

public class Program
{
    public static void Main(string[] args)
    {
        Server server = new Server("127.0.0.1", 19132);

        while (server.running) ;
    }
}