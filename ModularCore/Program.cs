using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace ModularCore {
    public class Program {

        // 复制文件夹
        private static void CopyFolder(string pathSource, string pathTarget) {

            // 复制子文件夹
            string[] dirs = System.IO.Directory.GetDirectories(pathSource);
            foreach (var dir in dirs) {
                string name = System.IO.Path.GetFileName(dir);
                CopyFolder(dir, $"{pathTarget}{it.SplitChar}{name}");
            }

            // 复制文件
            string[] files = System.IO.Directory.GetFiles(pathSource);
            foreach (var file in files) {
                string name = System.IO.Path.GetFileName(file);
                System.IO.File.Copy(file, $"{pathTarget}{it.SplitChar}{name}", true);
            }
        }

        // 安装所有的包
        private static void InstallPacks() {

            Console.WriteLine("[*] 初始化包管理 ...");

            string folderDown = $"{it.ExecPath}downloads";
            string folderPackage = $"{it.ExecPath}packages";
            if (!System.IO.Directory.Exists(folderDown)) System.IO.Directory.CreateDirectory(folderDown);
            if (!System.IO.Directory.Exists(folderPackage)) System.IO.Directory.CreateDirectory(folderPackage);

            // 判断并创建配置文件
            string fileXml = $"{folderPackage}{it.SplitChar}packages.xml";
            if (!System.IO.File.Exists(fileXml)) {
                Console.WriteLine("[*] 正在创建包管理文件 ...");
                using (var doc = new dpz3.Xml.XmlDocument()) {
                    var xml = new dpz3.Xml.XmlNode("xml");
                    doc.Nodes.Add(xml);
                    var packages = xml.AddNode("packages");
                    dpz3.File.UTF8File.WriteAllText(fileXml, doc.InnerXml);
                }
            }

            // 读取配置文件
            string szXml = dpz3.File.UTF8File.ReadAllText(fileXml);
            using (var doc = dpz3.Xml.Parser.GetDocument(szXml)) {
                var xml = doc["xml"];
                var packages = xml["packages"];
                foreach (var package in packages.GetNodesByTagName("package", false)) {
                    string packageName = package.Attr["name"];
                    string packageDownload = package.Attr["download"];
                    string packageInstall = package.Attr["install"];
                    Console.WriteLine($"[*] 包 {packageName} 下载版本:{packageDownload} 安装版本:{packageInstall}");
                    if (packageDownload != packageInstall) {
                        // 进行包的解压
                        Console.WriteLine($"[+] 包安装 {packageName} 版本:{packageDownload} ...");
                        string fileDown = $"{folderDown}{it.SplitChar}{packageName}-{packageDownload}.zip";
                        string folderInstall = $"{folderPackage}{it.SplitChar}{packageName}-{packageDownload}";
                        if (!System.IO.Directory.Exists(folderDown)) System.IO.Directory.CreateDirectory(folderDown);
                        ZipFile.ExtractToDirectory(fileDown, folderInstall);
                        // 进行包的安装
                        string folderRoot = $"{folderInstall}{it.SplitChar}wwwroot";
                        if (System.IO.Directory.Exists(folderRoot)) {
                            // 进行包内静态文件的复制
                            CopyFolder(folderRoot, $"{it.ExecPath}wwwroot");
                        }
                    }
                }
            }
        }

        public static void Main(string[] args) {

            // 快速通道初始化
            it.Initialize();

            // 安装包
            InstallPacks();

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
