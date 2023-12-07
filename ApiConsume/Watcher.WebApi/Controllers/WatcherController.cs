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

        [HttpGet("memory")]
        public ActionResult<string> GetMemoryUsage()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "VBoxManage",
                Arguments = $"guestcontrol yesubuntu run --exe /usr/bin/free --username ysert --password yesimsert --wait-stdout -- -m",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                double totalMemory = ParseTotalMemory(output);
                double usedMemory = ParseUsedMemory(output);
                double usagePercentage = (usedMemory / totalMemory) * 100;

                // Eşik değeri kontrolü yap
                string status = "Green";
                if (usagePercentage >= 80)
                {
                    status = "Red";
                }


                return Ok(new
                {
                    UsedMemory = usedMemory,
                    TotalMemory = totalMemory,
                    UsagePercentage = usagePercentage,
                    Status = status
                });
            }
        }

        private static double ParseUsedMemory(string output)
        {
            string[] lines = output.Split('\n');
            string usedMemoryLine = lines[1];

            string[] values = usedMemoryLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            double usedMemory = double.Parse(values[2]);

            return usedMemory;
        }

        private static double ParseTotalMemory(string output)
        {
            string[] lines = output.Split('\n');
            string totalMemoryLine = lines[1];

            string[] values = totalMemoryLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            double totalMemory = double.Parse(values[1]);

            return totalMemory;
        }
    }



}