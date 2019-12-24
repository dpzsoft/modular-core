using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

/// <summary>
/// 此应用的快捷使用通道
/// </summary>
public static partial class it {

    public class Session : dpz3.ssr.SessionClient, dpz3.Modular.ISessionManager {

        public const string S_EcpSessionId = "ycc_session_id";
        public const string S_EcpUserId = "ycc_user_id";
        public const string S_EcpUserName = "ycc_user_name";
        public const string S_EcpUserNick = "ycc_user_nick";
        public const string S_WeixinID = "weixin_id";
        public const string S_UserID = "user_id";

        public string EcpSessionId { get { return base[S_EcpSessionId]; } set { base[S_EcpSessionId] = value; } }
        public string EcpUserId { get { return base[S_EcpUserId]; } set { base[S_EcpUserId] = value; } }
        public string EcpUserName { get { return base[S_EcpUserName]; } set { base[S_EcpUserName] = value; } }
        public string EcpUserNick { get { return base[S_EcpUserNick]; } set { base[S_EcpUserNick] = value; } }
        public string WeixinID { get { return base[S_WeixinID]; } set { base[S_WeixinID] = value; } }
        public string UserID { get { return base[S_UserID]; } set { base[S_UserID] = value; } }

        /// <summary>
        /// 获取是否有效
        /// </summary>
        public bool Enabled { get; private set; }

        public Session(string sid = "", bool create = false) : base(it.Config.Ess.IPAddress, it.Config.Ess.Port, it.Config.Ess.Password) {
            if (create) {
                if (sid != "") {
                    if (!base.SetSessionID(sid)) base.CreateNewSessionID();
                } else {
                    base.CreateNewSessionID();
                }
                this.Enabled = true;
            } else {
                if (sid != "") {
                    this.Enabled = base.SetSessionID(sid);
                    //if (!base.SetSessionID(sid)) throw new Exception("交互信息无效或已过期");
                }
            }
        }

        /// <summary>
        /// 获取存储值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(string key) {
            //throw new NotImplementedException();
            return this[key];
        }

        /// <summary>
        /// 设置存储值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, string value) {
            //throw new NotImplementedException();
            this[key] = value;
        }
    }

}
