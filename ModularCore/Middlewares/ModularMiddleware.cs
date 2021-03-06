﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using dpz3;
using dpz3.Modular;
using System.IO.Compression;
using dpz3.db;
using Microsoft.AspNetCore.Http.Extensions;
using System.IO;
using System.Text;

namespace ModularCore.Middlewares {

    /// <summary>
    /// 模块化的接口定义
    /// </summary>
    public class ModularMethodInfo {

        /// <summary>
        /// 路由地址
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// 对象
        /// </summary>
        public Assembly Assembly { get; set; }

        /// <summary>
        /// 对象
        /// </summary>
        public System.Type Type { get; set; }

        /// <summary>
        /// 函数接口
        /// </summary>
        public MethodInfo Method { get; set; }

        /// <summary>
        /// 类名称
        /// </summary>
        public string ClassName { get; set; }
    }

    /// <summary>
    /// 模块化的宿主对象
    /// </summary>
    public class ModularHost : IHost, IDisposable {

        /// <summary>
        /// 超文本上下文
        /// </summary>
        public HttpContext Context { get; set; }

        /// <summary>
        /// 交互管理器
        /// </summary>
        public ISessionManager Session { get; set; }

        /// <summary>
        /// 数据库连接管理器
        /// </summary>
        public Connection Connection { get; set; }

        /// <summary>
        /// 宿主版本
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 工作目录
        /// </summary>
        public string WorkFolder { get; set; }

        /// <summary>
        /// 存储目录
        /// </summary>
        public string StorageFolder { get; set; }

        /// <summary>
        /// 包名称
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// 包版本
        /// </summary>
        public string PackageVersion { get; set; }

        /// <summary>
        /// 包工作目录
        /// </summary>
        public string PackageWorkFolder { get; set; }

        /// <summary>
        /// 调试输出
        /// </summary>
        /// <param name="content"></param>
        public void Debug(string content) {
            Program.Print(content);
        }

        /// <summary>
        /// 调试输出
        /// </summary>
        /// <param name="content"></param>
        public void DebugLine(string content = null) {
            if (content.IsNoneOrNull()) {
                Program.Println();
            } else {
                Program.Println(content);
            }
        }

        public void Dispose() {
            this.Context = null;
            this.Session = null;
            if (this.Connection != null) this.Connection.Dispose();
            //throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 模块化的包处理对象
    /// </summary>
    public class ModularPackage : dpz3.Object {

        private const string Platform_Name = "core";
        private const int Platform_Version = 1;

        public List<ModularMethodInfo> Posts { get; private set; }
        public List<ModularMethodInfo> Gets { get; private set; }
        private dpz3.KeyValues<string> _mimes;

        /// <summary>
        /// 获取包名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 获取包版本号
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// 获取工作目录
        /// </summary>
        public string WorkPath { get; private set; }

        /// <summary>
        /// 获取根目录
        /// </summary>
        public string RootPath { get; private set; }

        // 执行接口
        private Task ExecuteMethod(HttpContext httpContext, ModularMethodInfo info) {
            try {
                // 设置字符编码
                httpContext.Response.ContentType = "text/plain;charset=UTF-8";
                // 建立宿主对象
                using (ModularHost host = new ModularHost()) {
                    host.Version = it.Version;
                    host.WorkFolder = it.WorkPath;
                    host.StorageFolder = $"{it.WorkPath}storage";
                    host.PackageName = this.Name;
                    host.PackageVersion = this.Version;
                    host.PackageWorkFolder = this.WorkPath;
                    host.Context = httpContext;
                    if (!dpz3.Object.IsNull(it.Database.Entity)) host.Connection = new Connection(it.Database.Entity);
                    string returnType = info.Method.ReturnType.FullName;
                    switch (returnType) {
                        case "System.String":
                            // 处理返回为字符串的函数定义
                            if (info.ClassName == "ControllerBase") {
                                ControllerBase api = (ControllerBase)info.Assembly.CreateInstance(info.Type.FullName); ;
                                // 执行初始化调用
                                string res = api.Initialize(host);
                                if (!res.IsNoneOrNull()) return httpContext.Response.WriteAsync(res);
                                // 调用主事件
                                res = (string)info.Method.Invoke(api, null);
                                dpz3.Modular.Result.Text text = new dpz3.Modular.Result.Text() { Content = res };
                                return api.Render(text);
                            }
                            if (info.ClassName == "SessionControllerBase") {
                                SessionControllerBase api = (SessionControllerBase)info.Assembly.CreateInstance(info.Type.FullName); ;
                                // 执行初始化调用
                                string res = api.Initialize(host);
                                Program.Println($"[*] 当前交互标识 {api.SessionID}");
                                using (var session = new it.Session(api.SessionID)) {
                                    // 自动申请新的交互标识
                                    if (!session.Enable) {
                                        session.CreateNewSessionID();
                                        api.SessionID = session.SessionID;
                                        Program.Println($"[+] 生成了一个全新的交互标识 {api.SessionID}");
                                    }
                                    host.Session = session;
                                    if (!res.IsNoneOrNull()) return httpContext.Response.WriteAsync(res);
                                    // 调用主事件
                                    res = (string)info.Method.Invoke(api, null);
                                    dpz3.Modular.Result.Text text = new dpz3.Modular.Result.Text() { Content = res };
                                    return api.Render(text);
                                }
                            }
                            if (info.ClassName == "JttpSessionControllerBase") {
                                JttpSessionControllerBase api = (JttpSessionControllerBase)info.Assembly.CreateInstance(info.Type.FullName); ;
                                // 执行初始化调用
                                string res = api.Initialize(host);
                                using (var session = new it.Session(api.SessionID)) {
                                    host.Session = session;
                                    if (!res.IsNoneOrNull()) return httpContext.Response.WriteAsync(res);
                                    // 调用主事件
                                    res = (string)info.Method.Invoke(api, null);
                                    dpz3.Modular.Result.Text text = new dpz3.Modular.Result.Text() { Content = res };
                                    return api.Render(text);
                                }
                            }
                            return httpContext.Response.WriteAsync($"尚未支持的基础类型 {info.ClassName}");
                        case "dpz3.Modular.IResult":
                            // 处理返回为标准接口的函数定义
                            if (info.ClassName == "ControllerBase") {
                                ControllerBase api = (ControllerBase)info.Assembly.CreateInstance(info.Type.FullName); ;
                                // 执行初始化调用
                                string res = api.Initialize(host);
                                if (!res.IsNoneOrNull()) return httpContext.Response.WriteAsync(res);
                                // 调用主事件
                                IResult result = (IResult)info.Method.Invoke(api, null);
                                return api.Render(result);
                            }
                            if (info.ClassName == "SessionControllerBase") {
                                SessionControllerBase api = (SessionControllerBase)info.Assembly.CreateInstance(info.Type.FullName); ;
                                // 执行初始化调用
                                string res = api.Initialize(host);
                                Program.Println($"[*] 当前交互标识 {api.SessionID}");
                                using (var session = new it.Session(api.SessionID)) {
                                    // 自动申请新的交互标识
                                    if (!session.Enable) {
                                        session.CreateNewSessionID();
                                        api.SessionID = session.SessionID;
                                        Program.Println($"[+] 生成了一个全新的交互标识 {api.SessionID}");
                                    }
                                    host.Session = session;
                                    if (!res.IsNoneOrNull()) return httpContext.Response.WriteAsync(res);
                                    // 调用主事件
                                    IResult result = (IResult)info.Method.Invoke(api, null);
                                    return api.Render(result);
                                }
                            }
                            if (info.ClassName == "JttpSessionControllerBase") {
                                JttpSessionControllerBase api = (JttpSessionControllerBase)info.Assembly.CreateInstance(info.Type.FullName); ;
                                // 执行初始化调用
                                string res = api.Initialize(host);
                                using (var session = new it.Session(api.SessionID)) {
                                    host.Session = session;
                                    if (!res.IsNoneOrNull()) return httpContext.Response.WriteAsync(res);
                                    // 调用主事件
                                    IResult result = (IResult)info.Method.Invoke(api, null);
                                    return api.Render(result);
                                }
                            }
                            return httpContext.Response.WriteAsync($"尚未支持的基础类型 {info.ClassName}");
                        default:
                            return httpContext.Response.WriteAsync($"尚未支持的返回类型 {returnType}");
                    }
                }
            } catch (Exception ex) {
                return httpContext.Response.WriteAsync(ex.ToString());
            }
        }

        public Task Invoke(HttpContext context, string path) {
            // 获取申请器
            var request = context.Request;
            // Program.Println($"[*] 包 {this.Name} 接受 {request.Method} {path} ...");
            // 执行事件
            switch (request.Method) {
                case "POST":
                    // 遍历所有注册接口
                    foreach (var info in Posts) {
                        if (path == info.Route) {
                            return ExecuteMethod(context, info);
                        }
                    }
                    break;
                case "GET":
                    // 遍历所有注册接口
                    foreach (var info in Gets) {
                        if (path == info.Route) {
                            return ExecuteMethod(context, info);
                        }
                    }
                    // 判断是否存在静态文件
                    string filePath = $"{this.RootPath}{path.Replace('/', it.SplitChar)}";
                    Program.Println($"[*] 查找静态文件 {filePath} ...");
                    if (System.IO.File.Exists(filePath)) {
                        Program.Println($"[*] 输出文件 {filePath} ...");
                        string ext = System.IO.Path.GetExtension(filePath).ToLower();
                        if (_mimes.ContainsKey(ext)) {
                            // Program.Println($"[*] 输出mime {_mimes[ext]} ...");
                            context.Response.ContentType = _mimes[ext];
                        }
                        return context.Response.SendFileAsync(filePath);
                    }
                    break;
            }
            return null;
        }

        // 注册类库
        private void RegLibrary(string path) {

            Program.Println($"    正在注册类库 {path} ...");

            // 加载类库
            Assembly assembly = Assembly.LoadFile(path);

            // 遍历类库中的所有类
            var tps = assembly.GetTypes();
            foreach (var tp in tps) {
                // 遍历类型特性，判断类是否满足条件
                var attrs = tp.GetCustomAttributes();
                foreach (var attr in attrs) {
                    if (attr.GetType().FullName == "dpz3.Modular.ModularAttribute") {
                        // 重建模块化特性
                        dpz3.Modular.ModularAttribute modular = (dpz3.Modular.ModularAttribute)attr;
                        // 找到Api定义类
                        switch (modular.ModularType) {
                            case ModularTypes.Api:
                            case ModularTypes.Session:
                            case ModularTypes.SessionApi:
                                // 读取路由地址定义
                                string route = modular.Route.Replace("{ControllerName}", tp.Name);
                                // 确定类名称
                                string className = "";
                                switch (modular.ModularType) {
                                    case ModularTypes.Api: className = "ControllerBase"; break;
                                    case ModularTypes.Session: className = "SessionControllerBase"; break;
                                    case ModularTypes.SessionApi: className = "JttpSessionControllerBase"; break;
                                }
                                // 遍历所有的函数定义
                                var methods = tp.GetMethods();
                                foreach (var method in methods) {
                                    // 遍历函数特性，判断函数是否满足条件
                                    var methodAttrs = method.GetCustomAttributes();
                                    foreach (var methodAttr in methodAttrs) {
                                        if (methodAttr.GetType().FullName == "dpz3.Modular.ModularAttribute") {
                                            // 重建模块化特性
                                            dpz3.Modular.ModularAttribute methodModular = (dpz3.Modular.ModularAttribute)methodAttr;
                                            string routeNew = $"{route}/{methodModular.Route}".ToLower();
                                            ModularMethodInfo methodInfo = new ModularMethodInfo() {
                                                Method = method,
                                                Assembly = assembly,
                                                Type = tp,
                                                Route = routeNew,
                                                ClassName = className,
                                            };
                                            switch (methodModular.ModularType) {
                                                case dpz3.Modular.ModularTypes.Post:
                                                    // 添加一条POST接口信息
                                                    Program.Println($"[@] POST /{this.Name}{routeNew} ...");
                                                    Posts.Add(methodInfo);
                                                    break;
                                                case dpz3.Modular.ModularTypes.Get:
                                                    // 添加一条GET接口信息
                                                    Program.Println($"[@] GET /{this.Name}{routeNew} ...");
                                                    Gets.Add(methodInfo);
                                                    // 处理特殊的首页
                                                    if (routeNew.EndsWith("/index")) {
                                                        routeNew = routeNew.Substring(0, routeNew.Length - 5);
                                                        // 添加一条GET接口信息
                                                        Program.Println($"[@] GET /{this.Name}{routeNew} ...");
                                                        Gets.Add(new ModularMethodInfo() {
                                                            Method = method,
                                                            Assembly = assembly,
                                                            Type = tp,
                                                            Route = routeNew,
                                                            ClassName = className,
                                                        });
                                                    }
                                                    break;
                                            }
                                            // 结束特性循环
                                            break;
                                        }
                                    }
                                }
                                break;
                        }
                        // 结束特性循环
                        break;
                    }
                }
            }
        }

        public ModularPackage(dpz3.KeyValues<string> mimes, string folderPackage, string packageName, string packageVersion) {
            // 初始化接口列表信息
            Posts = new List<ModularMethodInfo>();
            Gets = new List<ModularMethodInfo>();
            _mimes = mimes;
            // 读取包信息
            this.Name = packageName;
            this.Version = packageVersion;
            string folderInstall = $"{folderPackage}{it.SplitChar}{packageName}{it.SplitChar}{packageVersion}";
            this.WorkPath = folderInstall;
            this.RootPath = $"{folderInstall}{it.SplitChar}wwwroot";
            Program.Println($"[*] 读取包版本 {packageName} 安装版本:{packageVersion}");
            // 判断依赖有效性
            string pathCfg = $"{folderInstall}{it.SplitChar}modular.json";
            Program.Println($"    读取包配置文件 {pathCfg} ...");
            bool isEnable = false;
            int plmVersion = 0;
            if (System.IO.File.Exists(pathCfg)) {
                string szJson = dpz3.File.UTF8File.ReadAllText(pathCfg);
                using (var json = dpz3.Json.Parser.ParseJson(szJson)) {
                    string jsonName = json.Str["name"];
                    string jsonVersion = json.Str["version"];
                    string jsonDescription = json.Str["description"];
                    var jsonDepends = json.Arr["depend"];
                    // Program.Println($"    获取包 {packageName} 信息");
                    Program.Println($"    名称:{jsonName}");
                    Program.Println($"    版本:{jsonVersion}");
                    Program.Println($"    描述:{jsonDescription}");
                    for (int i = 0; i < jsonDepends.Count; i++) {
                        var depend = jsonDepends.Obj[i];
                        string dependPlatform = depend.Str["platform"];
                        int dependVersion = depend.Int["version"];
                        Program.Println($"    依赖 => {dependPlatform}:{dependVersion}");
                        if (dependPlatform == Platform_Name) {
                            plmVersion = dependVersion;
                        }
                    }
                }
            }
            if (plmVersion < Platform_Version) {
                Program.Println($"[-] 包 {packageName} 依赖关系不符合: 此包适用于旧版本主程序，请更新包或联系开发者进行更新");
            } else if (plmVersion < Platform_Version) {
                Program.Println($"[-] 包 {packageName} 依赖关系不符合: 此包适用于更新版本的主程序，请升级主程序");
            } else {
                isEnable = true;
            }
            if (isEnable) {
                // 注册控制器
                string pathDll = $"{folderInstall}{it.SplitChar}controller.dll";
                if (System.IO.File.Exists(pathDll)) {
                    Program.Println($"    包 {packageName} 控制器激活中 ...");
                    RegLibrary(pathDll);
                }
            }
        }
    }

    public class ModularMiddleware {

        private readonly RequestDelegate _next;
        private List<ModularPackage> _packages;

        private dpz3.KeyValues<string> _mimes;

        // 复制文件夹
        private void CopyFolder(string pathSource, string pathTarget) {

            // 复制子文件夹
            string[] dirs = System.IO.Directory.GetDirectories(pathSource);
            foreach (var dir in dirs) {
                string name = System.IO.Path.GetFileName(dir);
                string pathNew = $"{pathTarget}{it.SplitChar}{name}";
                Program.Println($"[+] 创建目录 {pathNew} ...");
                if (!System.IO.Directory.Exists(pathNew)) System.IO.Directory.CreateDirectory(pathNew);
                CopyFolder(dir, pathNew);
            }

            // 复制文件
            string[] files = System.IO.Directory.GetFiles(pathSource);
            foreach (var file in files) {
                string name = System.IO.Path.GetFileName(file);
                string pathNew = $"{pathTarget}{it.SplitChar}{name}";
                Program.Println($"[+] 拷贝文件 {pathNew} ...");
                System.IO.File.Copy(file, $"{pathTarget}{it.SplitChar}{name}", true);
            }
        }

        // 安装所有的包
        private void InstallPacks() {

            Program.Println("[*] 初始化包管理 ...");

            string folderRoot = $"{it.ExecPath}wwwroot";
            string folderDown = $"{it.ExecPath}downloads";
            string folderPackage = $"{it.ExecPath}packages";
            if (!System.IO.Directory.Exists(folderRoot)) System.IO.Directory.CreateDirectory(folderRoot);
            if (!System.IO.Directory.Exists(folderDown)) System.IO.Directory.CreateDirectory(folderDown);
            if (!System.IO.Directory.Exists(folderPackage)) System.IO.Directory.CreateDirectory(folderPackage);

            // 判断并创建配置文件
            string fileXml = $"{folderPackage}{it.SplitChar}packages.xml";
            if (!System.IO.File.Exists(fileXml)) {
                Program.Println("[*] 正在创建包管理文件 ...");
                using (var doc = new dpz3.Xml.XmlDocument()) {
                    var xml = new dpz3.Xml.XmlNode("xml");
                    doc.Nodes.Add(xml);
                    var packages = xml.AddNode("packages");
                    dpz3.File.UTF8File.WriteAllText(fileXml, doc.InnerXml);
                }
            }

            // 读取配置文件
            string szXml = dpz3.File.UTF8File.ReadAllText(fileXml);
            bool isUpdate = false;
            using (var doc = dpz3.Xml.Parser.GetDocument(szXml)) {
                var xml = doc["xml"];
                var packages = xml["packages"];
                foreach (var package in packages.GetNodesByTagName("package", false)) {
                    string packageName = package.Attr["name"];
                    string packageInstall = package.Attr["version"];
                    // 添加一个包信息
                    _packages.Add(new ModularPackage(_mimes, folderPackage, packageName, packageInstall));
                }
                // 保存配置
                if (isUpdate) dpz3.File.UTF8File.WriteAllText(fileXml, doc.InnerXml);
            }
        }

        // 加载mime集合
        private void LoadMimes() {
            string path = $"{it.Config.WorkFolder}mime.xml";
            if (!System.IO.File.Exists(path)) {
                using (var doc = new dpz3.Xml.XmlDocument()) {
                    var xml = new dpz3.Xml.XmlNode("xml");
                    doc.Nodes.Add(xml);
                    var mime = xml.AddNode("mime");
                    mime.Attr["extension"] = ".html";
                    mime.Attr["value"] = "text/html";
                    dpz3.File.UTF8File.WriteAllText(path, doc.InnerXml);
                }
            }
            string content = dpz3.File.UTF8File.ReadAllText(path);
            using (var doc = new dpz3.Xml.XmlDocument(content)) {
                var xml = doc["xml"];
                var mimes = xml.GetNodesByTagName("mime", false);
                foreach (var mime in mimes) {
                    _mimes[mime.Attr["extension"].ToLower()] = mime.Attr["value"];
                }
            }
        }

        public ModularMiddleware(RequestDelegate next) {
            _next = next;
            _packages = new List<ModularPackage>();
            _mimes = new KeyValues<string>();

            // 加载mime集合
            LoadMimes();

            // 安装包文件
            InstallPacks();
            //string path = $"{it.ExecPath}lib{it.SplitChar}ClassDemo.dll";
            //RegLibrary(path);
        }

        #region [=====特殊路径处理=====]

        public Task ReturnMap(HttpContext context) {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.Append("<head>");
            sb.Append("<meta charset=\"utf-8\" />");
            sb.Append("<title>模块地图</title>");
            sb.Append("</head>");
            sb.Append("<body>");
            sb.Append("<div>");
            sb.Append("<dl>");
            // 遍历所有包
            foreach (var package in _packages) {
                sb.AppendFormat("<dt>包 {0}</dt>", package.Name);
                foreach (var info in package.Gets) {
                    sb.AppendFormat("<dd>GET <a href=\"/{0}{1}\" target=\"_blank\">/{0}{1}<a></dd>", package.Name, info.Route);
                }
                foreach (var info in package.Posts) {
                    sb.AppendFormat("<dd>POST /{0}{1}</dd>", package.Name, info.Route);
                }
            }
            sb.Append("</dl>");
            sb.Append("</div>");
            sb.Append("</body>");
            sb.Append("</html>");
            return context.Response.WriteAsync(sb.ToString());
        }

        #endregion

        public Task Invoke(HttpContext httpContext) {
            // 获取申请器
            var request = httpContext.Request;
            // 读取访问地址
            string path = request.Path.ToString().ToLower();
            Program.Println($"[*] {request.Method} {path}");
            // 遍历所有包
            foreach (var package in _packages) {
                if (path.StartsWith($"/{package.Name}/")) {
                    return package.Invoke(httpContext, path.Substring(package.Name.Length + 1));
                }
            }
            // 处理特殊路径
            var siteInfo = it.Config.Site.GetInfo("default");
            if (siteInfo.IsDevelop) {
                if (path == "/map") return ReturnMap(httpContext);
            }
            // 未找到接口，返回404
            httpContext.Response.StatusCode = 404;
            return httpContext.Response.StartAsync();
            // return _next(httpContext);
        }

    }

    public static class ModularMiddlewareExtensions {
        public static IApplicationBuilder UseCrossDomainMiddleware(this IApplicationBuilder builder) {
            return builder.UseMiddleware<ModularMiddleware>();
        }
    }

}
