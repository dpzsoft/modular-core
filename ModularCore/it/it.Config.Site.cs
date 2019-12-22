using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

/// <summary>
/// 此应用的快捷使用通道
/// </summary>
public static partial class it {

    public static partial class Config {

        public class SiteInfo {

            internal string WebUrl { get; set; }
            internal string WebSocketUrl { get; set; }
            internal string Storage { get; set; }

        }

        public static class Site {

            private static dpz3.File.ConfFile _cfg;

            public static SiteInfo GetInfo (string domain) {
                var group = _cfg[domain];
                if (dpz3.Object.IsNull (group)) group = _cfg["default"];
                return new SiteInfo () {
                    WebUrl = group["url"],
                        WebSocketUrl = group["ws"],
                        Storage = group["storage"]
                };
            }

            /// <summary>
            /// 使用初始化
            /// </summary>
            public static void Initialize (string path) {

                // 创建默认配置
                if (!System.IO.File.Exists (path)) {
                    using (var cfg = new dpz3.File.ConfFile (path)) {
                        var group = cfg["default"];
                        group["url"] = "http://127.0.0.1:8080";
                        group["ws"] = "";
                        group["storage"] = "";
                        cfg.Save ();
                    }
                }

                _cfg = new dpz3.File.ConfFile (path);

            }

        }
    }

}