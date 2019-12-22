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

        /// <summary>
        /// 工作文件夹
        /// </summary>
        internal static string WorkFolder { get; private set; }

        /// <summary>
        /// 使用初始化
        /// </summary>
        internal static void Initialize(string path) {
            if (!path.EndsWith(it.SplitChar)) path += it.SplitChar;
            it.Config.WorkFolder = path;

            // 初始化配置网站信息
            it.Config.Site.Initialize($"{path}site.cfg");

            // 初始化配置APi信息
            it.Config.Api.Initialize($"{path}api.cfg");

            // 初始化配置Ess交互信息
            it.Config.Ess.Initialize($"{path}ess.cfg");
        }
    }

}