using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using dpz3;

namespace ModularCore {
    public class Program {

        public static void Main(string[] args) {

            // 快速通道初始化
            it.Initialize();

            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// 输出内容
        /// </summary>
        /// <param name="content"></param>
        public static void Print(string content) {
            if (!it.Config.Site.Default.IsDevelop) return;
            Console.Write(content);
        }

        /// <summary>
        /// 输出内容
        /// </summary>
        /// <param name="content"></param>
        public static void Println(string content) {
            if (content.IsNoneOrNull()) Println();
            if (!it.Config.Site.Default.IsDevelop) return;
            Console.WriteLine(content);
        }

        /// <summary>
        /// 输出内容
        /// </summary>
        /// <param name="content"></param>
        public static void Println() {
            if (!it.Config.Site.Default.IsDevelop) return;
            Console.WriteLine();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    string pathKestrel = $"{it.Config.WorkFolder}kestrel.cfg";
                    Console.WriteLine($"[*] 应用配置 {pathKestrel} ...");
                    dpz3.AspNetCore.Kestrel.DeployConfig(webBuilder, pathKestrel);
                    webBuilder.UseStartup<Startup>();
                });
    }
}
