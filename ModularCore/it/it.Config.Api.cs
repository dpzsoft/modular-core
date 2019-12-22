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

        public class ApiInfo {

            public string Url { get; private set; }
            public string Id { get; private set; }
            public string Key { get; private set; }

            public ApiInfo (dpz3.File.Conf.SettingGroup group) {
                this.Url = group["url"];
                this.Id = group["id"];
                this.Key = group["key"];
            }

        }

        public static class Api {

            public static ApiInfo Ecp { get; private set; }

            /// <summary>
            /// 使用初始化
            /// </summary>
            internal static void Initialize (string path) {

                // 创建默认配置
                if (!System.IO.File.Exists (path)) {
                    using (var cfg = new dpz3.File.ConfFile (path)) {
                        var group = cfg["Ecp"];
                        group["url"] = "https://v5.lywos.com";
                        group["id"] = "";
                        group["key"] = "";
                        cfg.Save ();
                    }
                }

                using (var cfg = new dpz3.File.ConfFile (path)) {
                    Ecp = new ApiInfo (cfg["Ecp"]);
                }

            }

        }
    }

}