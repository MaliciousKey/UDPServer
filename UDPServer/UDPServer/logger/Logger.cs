using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

public class Logger
{
    public DateTime timeInterval;
    public List<string> logs;

    public Logger()
    {
        timeInterval = DateTime.UtcNow;
        logs = new List<string>();
    }

    public void Log(string? log, Thread thread)
    {
        string toLog = this.Construct(log, thread);
        logs.Add(toLog);
        Console.WriteLine(toLog);
    }

    private string Construct(string? log, Thread thread)
    {
        return string.Format("[{0}:{1}:{2}] [" + thread.Name + "] " + log, new object[]
        {
            Math.Round((double)timeInterval.Minute),
            Math.Round((double)timeInterval.Second),
            Math.Round((double)timeInterval.Millisecond)
        });
    }

    public void SaveLog(string logName)
    {
        StringBuilder sb = new StringBuilder(0);
        File.WriteAllLines(logName, logs.ToArray());
    }
}
