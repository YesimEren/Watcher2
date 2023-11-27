using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Watcher.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WatcherController : ControllerBase
    {
        private bool SanalMakina() //IsVirtualMachineRunning
        {
            string sanalmakinadi = "yesubuntu"; 
            string output = VBoxManageCommand($"showvminfo \"{sanalmakinadi}\"");

            return output.Contains("running");
        }

        private string VBoxManageCommand(string arguments) 
        {
            Process process = new Process();
            process.StartInfo.FileName = "VBoxManage";
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }

        private void LogToFile(string logMessage)
        {
            string logFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Log.txt"); //string logFile = Path.Combine(Directory.GetCurrentDirectory(), "Log.txt");
            using (StreamWriter writer = System.IO.File.AppendText(logFile))
            {
                writer.WriteLine($"{DateTime.Now} - {logMessage}");
            }
        }

        [HttpGet("status")]
        public IActionResult VBoxStatus() 
        {
            bool isRunning = SanalMakina();
            return Ok(new { IsRunning = isRunning });
        }

        [HttpPost("start")]
        public IActionResult StartVirtualMachine()
        {
            string sanalmakinadi = "yesubuntu";
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "VBoxManage",
                Arguments = $"startvm \"{sanalmakinadi}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = processStartInfo;
                process.Start();
                process.WaitForExit();
            }

            LogToFile("Virtual Machine started");

            return Ok(new { Message = "Virtual Machine started successfully" });
        }

        [HttpPost("stop")]
        public IActionResult StopVirtualMachine()
        {
            string sanalmakinadi = "yesubuntu";
            Process process = new Process();
            process.StartInfo.FileName = "VBoxManage";
            process.StartInfo.Arguments = $"controlvm \"{sanalmakinadi}\" poweroff";
            process.Start();
            process.WaitForExit();

            LogToFile("Virtual Machine stopped "); // Buraya sanal makinanın adını ekle sonra.(ubuntuyes sanal amkinası duruldu gibi)

            return Ok(new { Message = "Virtual Machine stopped successfully " });
        }

    }
}
