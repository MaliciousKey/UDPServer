using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Command
{
    private string command;
    private string[]? args;

    public Command(string command, string[]? args)
    {
        this.command = command;
        this.args = args;
    }

    //public void Execute()

    public string GetArgument(int index)
    {
        return args[index];
    }

    public string[]? GetArguments()
    {
        return args;
    }

    public string GetInput()
    {
        return command;
    }
}
