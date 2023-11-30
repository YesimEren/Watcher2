using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WatcherBusinessLayer.Contracts;

namespace Watcher.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WatcherController : ControllerBase
    {
        private readonly IWatcherService _watcherService;

        public WatcherController(IWatcherService watcherService)
        {
            _watcherService = watcherService;
        }
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

        private (int total, int used) GetMemoryInfoFromVirtualMachine()
        {
            string sanalMakinaAdi = "yesubuntu";
            string komut = "free -m";
            string output = VBoxManageGuestControlCommand(sanalMakinaAdi, komut);

            // Çıktıyı analiz etmek ve total, used değerlerini çıkarmak için gerekli kod buraya eklenecek

            // Örnek: 
            // total       used
            // 7937        252
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
        public IActionResult GetMemoryUsage()
        {
            try
            {
                var (total, used) = GetMemoryInfoFromVirtualMachine();
                float memoryUsagePercentage = (float)used / total * 100;

                // Bellek kullanımı eşiği aşılmıyorsa, yeşil ışığı aç
                UpdateMemoryStatus(memoryUsagePercentage);

                return Ok(new { Total = total, Used = used, UsagePercentage = memoryUsagePercentage });
            }
            catch (Exception ex)
            {
                LogToFile($"Error in GetMemoryUsage: {ex.Message}");

                // Hata durumunda kırmızı ışığı aç
                UpdateMemoryStatus(100); // Hata durumunu temsil eden %100 kullanım

                return StatusCode(500, new { Message = "An error occurred while getting memory usage." });
            }
        }

        private void UpdateMemoryStatus(float memoryUsagePercentage)
        {
            // Bellek kullanımı eşiği aşılmıyorsa, yeşil ışığı aç
            const float greenThreshold = 80;
            const float redThreshold = 100; // Hata durumunu temsil eden %100 kullanım

            const string greenColor = "green";
            const string redColor = "red";

            // Işığın rengini belirle
            string color = memoryUsagePercentage > greenThreshold ? redColor : greenColor;

            // Işığın rengini güncelle
            const string vmStatusElementId = "vmStatus";
            const string kundenportalElementId = "upperDiv";

            const string scriptFormat = "document.getElementById('{0}').style.backgroundColor = '{1}';";
            string vmStatusScript = string.Format(scriptFormat, vmStatusElementId, color);
            string kundenportalScript = string.Format(scriptFormat, kundenportalElementId, color);

            // JavaScript ile tarayıcıdaki ışığı güncelle
            string updateStatusScript = vmStatusScript + kundenportalScript;
            Response.WriteAsync($"<script>{updateStatusScript}</script>");
        }

    }
}