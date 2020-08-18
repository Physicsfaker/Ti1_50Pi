using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Unix.Native;

namespace Ti_150Pi.HardwareClasses
{
    public static class Clock
    {

        public static void Set(ushort hour, ushort min, ushort sec, ushort day, ushort month, ushort year)
        {
            if (hour < 0 || hour > 24) hour = 12;
            if (min < 0 || min > 59) min = 0;
            if (sec < 0 || sec > 59) sec = 0;
            if (day < 0 || day > 31) day = 1;
            if (month < 1 || month > 12) month = 1;
            if (year < 2020 || year > 2099) year = 2020;


            ExecuteBashCommand($"sudo date -s \"{month}/{day}/{year}\"");
            ExecuteBashCommand($"sudo date -s \"{hour}:{min}:{sec}\"");
            ExecuteBashCommand("sudo hwclock -w");
        }


        private static void ExecuteBashCommand(string command)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"" + command + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            proc.WaitForExit();
        }
    }
}
