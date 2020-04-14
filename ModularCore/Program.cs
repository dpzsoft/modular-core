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

        private static dpz3.Logger logger;

        public static void Main(string[] args) {

            // ����ͨ����ʼ��
            it.Initialize();

            // ��ʼ����־������
            string logPath = $"{it.ExecPath}log";
            if (!System.IO.Directory.Exists(logPath)) System.IO.Directory.CreateDirectory(logPath);
            logger = new Logger(logPath);

            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// �������
        /// </summary>
        /// <param name="content"></param>
        public static void Print(string content) {
            // �����־
            logger.Write(content);
            if (!it.Config.Site.Default.IsDevelop) return;
            Console.Write(content);
        }

        /// <summary>
        /// �������
        /// </summary>
        /// <param name="content"></param>
        public static void Println(string content) {
            string cnt = $"{dpz3.Time.Now.ToTimeString()} {content}\r\n";
            logger.Write(cnt);
            if (content.IsNoneOrNull()) Println();
            if (!it.Config.Site.Default.IsDevelop) return;
            Console.WriteLine(cnt);
        }

        /// <summary>
        /// �������
        /// </summary>
        /// <param name="content"></param>
        public static void Println() {
            if (!it.Config.Site.Default.IsDevelop) return;
            Console.WriteLine();
        }

        public static void Log() {

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    string pathKestrel = $"{it.Config.WorkFolder}kestrel.cfg";
                    Println($"[*] Ӧ������ {pathKestrel} ...");
                    dpz3.AspNetCore.Kestrel.DeployConfig(webBuilder, pathKestrel);
                    webBuilder.UseStartup<Startup>();
                });
    }
}
