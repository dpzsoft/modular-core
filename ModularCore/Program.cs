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

            // ����ͨ����ʼ��
            it.Initialize();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    string pathKestrel = $"{it.Config.WorkFolder}kestrel.cfg";
                    Console.WriteLine($"[*] Ӧ������ {pathKestrel} ...");
                    dpz3.AspNetCore.Kestrel.DeployConfig(webBuilder, pathKestrel);
                    webBuilder.UseStartup<Startup>();
                });
    }
}
