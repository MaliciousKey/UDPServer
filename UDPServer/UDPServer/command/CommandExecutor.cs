using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CommandExecutor
{
    public CommandExecutor()
    {

    }

    public Command ListenForCommand()
    {
        string? input = Console.ReadLine();
        string[]? args = input.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

        return new Command(input, args);
    }

    public void Execute(Command command, Server server)
    {
        if(command.GetInput() == "stop")
        {
            server.CloseServer();
        }
    }
} 
