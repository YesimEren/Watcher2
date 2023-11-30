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
            string sanalMakinaAdi = "yesubuntu";
            string komut = "free -m";
            string output = VBoxManageGuestControlCommand(sanalMakinaAdi, komut);

            string[] lines = output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            string[] values = lines[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            int total = int.Parse(values[1]);
            int used = int.Parse(values[2]);

            return (total, used);
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