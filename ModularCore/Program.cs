using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ModularCore {
    public class Program {

        public static void Main(string[] args) {

            // 快速通道初始化
            it.Initialize();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    dpz3.AspNetCore.Kestrel.DeployConfig($"{it.Config.WorkFolder}kestrel.cfg", webBuilder);
                    webBuilder.UseStartup<Startup>();
                });
    }
}
