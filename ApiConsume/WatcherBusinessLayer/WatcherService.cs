using System;
using System.Diagnostics;
using Microsoft.Diagnostics.NETCore.Client;
using WatcherBusinessLayer.Contracts;

namespace WatcherBusinessLayer
{
    public class WatcherService : IWatcherService
    {
        public (int total, int used) GetMemoryInfoFromVirtualMachine()
        {
            string sanalmakinadi = "yesubuntu";
            string komut = "free -m";
            string output = VBoxManageGuestControlCommand(sanalmakinadi, komut);

            string[] lines = output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length >= 2)
            {
                string[] values = lines[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length >= 3)
                {
                    int total = int.Parse(values[1]);
                    int used = int.Parse(values[2]);

                    return (total, used);
                }
                

            }
            return (0, 0);


        }

        private string VBoxManageGuestControlCommand(string vmName, string command)
        {
            Process process = new Process();
            process.StartInfo.FileName = "VBoxManage";
            process.StartInfo.Arguments = $"guestcontrol {vmName} run --exe /bin/bash --ysert --yesimsert --wait-stdout -- {command}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }


    }
}