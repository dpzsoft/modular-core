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

            // ����ͨ����ʼ��
            it.Initialize();

            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// �������
        /// </summary>
        /// <param name="content"></param>
        public static void Print(string content) {
            if (!it.Config.Site.Default.IsDevelop) return;
            Console.Write(content);
        }

        /// <summary>
        /// �������
        /// </summary>
        /// <param name="content"></param>
        public static void Println(string content) {
            if (content.IsNoneOrNull()) Println();
            if (!it.Config.Site.Default.IsDevelop) return;
            Console.WriteLine(content);
        }

        /// <summary>
        /// �������
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
                    Console.WriteLine($"[*] Ӧ������ {pathKestrel} ...");
                    dpz3.AspNetCore.Kestrel.DeployConfig(webBuilder, pathKestrel);
                    webBuilder.UseStartup<Startup>();
                });
    }
}
