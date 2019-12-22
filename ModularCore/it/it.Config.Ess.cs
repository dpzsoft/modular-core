using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using dpz3;

/// <summary>
/// 此应用的快捷使用通道
/// </summary>
public static partial class it {

    public static partial class Config {

        public static class Ess {

            public static string IPAddress { get; private set; }
            public static int Port { get; private set; }
            public static string Password { get; private set; }

            /// <summary>
            /// 使用初始化
            /// </summary>
            internal static void Initialize(string path) {

                // 创建默认配置
                if (!System.IO.File.Exists(path)) {
                    using (var cfg = new dpz3.File.ConfFile(path)) {
                        var group = cfg["default"];
                        group["ip"] = "127.0.0.1";
                        group["port"] = "8601";
                        group["password"] = "000000";
                        cfg.Save();
                    }
                }

                // 读取配置
                using (var cfg = new dpz3.File.ConfFile(path)) {
                    var group = cfg["default"];
                    IPAddress = group["ip"];
                    Port = group["port"].ToInteger();
                    Password = group["password"];
                }

            }

        }
    }

}