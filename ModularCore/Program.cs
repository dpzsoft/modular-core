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

        // �����ļ���
        private static void CopyFolder(string pathSource, string pathTarget) {

            // �������ļ���
            string[] dirs = System.IO.Directory.GetDirectories(pathSource);
            foreach (var dir in dirs) {
                string name = System.IO.Path.GetFileName(dir);
                CopyFolder(dir, $"{pathTarget}{it.SplitChar}{name}");
            }

            // �����ļ�
            string[] files = System.IO.Directory.GetFiles(pathSource);
            foreach (var file in files) {
                string name = System.IO.Path.GetFileName(file);
                System.IO.File.Copy(file, $"{pathTarget}{it.SplitChar}{name}", true);
            }
        }

        // ��װ���еİ�
        private static void InstallPacks() {

            Console.WriteLine("[*] ��ʼ�������� ...");

            string folderDown = $"{it.ExecPath}downloads";
            string folderPackage = $"{it.ExecPath}packages";
            if (!System.IO.Directory.Exists(folderDown)) System.IO.Directory.CreateDirectory(folderDown);
            if (!System.IO.Directory.Exists(folderPackage)) System.IO.Directory.CreateDirectory(folderPackage);

            // �жϲ����������ļ�
            string fileXml = $"{folderPackage}{it.SplitChar}packages.xml";
            if (!System.IO.File.Exists(fileXml)) {
                Console.WriteLine("[*] ���ڴ����������ļ� ...");
                using (var doc = new dpz3.Xml.XmlDocument()) {
                    var xml = new dpz3.Xml.XmlNode("xml");
                    doc.Nodes.Add(xml);
                    var packages = xml.AddNode("packages");
                    dpz3.File.UTF8File.WriteAllText(fileXml, doc.InnerXml);
                }
            }

            // ��ȡ�����ļ�
            string szXml = dpz3.File.UTF8File.ReadAllText(fileXml);
            using (var doc = dpz3.Xml.Parser.GetDocument(szXml)) {
                var xml = doc["xml"];
                var packages = xml["packages"];
                foreach (var package in packages.GetNodesByTagName("package", false)) {
                    string packageName = package.Attr["name"];
                    string packageDownload = package.Attr["download"];
                    string packageInstall = package.Attr["install"];
                    Console.WriteLine($"[*] �� {packageName} ���ذ汾:{packageDownload} ��װ�汾:{packageInstall}");
                    if (packageDownload != packageInstall) {
                        // ���а��Ľ�ѹ
                        Console.WriteLine($"[+] ����װ {packageName} �汾:{packageDownload} ...");
                        string fileDown = $"{folderDown}{it.SplitChar}{packageName}-{packageDownload}.zip";
                        string folderInstall = $"{folderPackage}{it.SplitChar}{packageName}-{packageDownload}";
                        if (!System.IO.Directory.Exists(folderDown)) System.IO.Directory.CreateDirectory(folderDown);
                        ZipFile.ExtractToDirectory(fileDown, folderInstall);
                        // ���а��İ�װ
                        string folderRoot = $"{folderInstall}{it.SplitChar}wwwroot";
                        if (System.IO.Directory.Exists(folderRoot)) {
                            // ���а��ھ�̬�ļ��ĸ���
                            CopyFolder(folderRoot, $"{it.ExecPath}wwwroot");
                        }
                    }
                }
            }
        }

        public static void Main(string[] args) {

            // ����ͨ����ʼ��
            it.Initialize();

            // ��װ��
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
