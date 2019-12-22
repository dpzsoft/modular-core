using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

/// <summary>
/// 此应用的快捷使用通道
/// </summary>
public static partial class it {

    internal static partial class Database {

        /// <summary>
        /// 获取维护数据库定义
        /// </summary>
        public static dpz3.db.Database Entity { get; private set; }

        /// <summary>
        /// 使用初始化
        /// </summary>
        public static void Initialize() {

            Entity = dpz3.db.Database.LoadFromConf(it.Config.WorkFolder + "db.cfg", "entity");

        }

    }

}
