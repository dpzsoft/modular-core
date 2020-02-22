using System;
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

        private List<ModularMethodInfo> _post;
        private List<ModularMethodInfo> _get;
        private dpz3.KeyValues<string> _mimes;

        /// <summary>
        /// 获取包名称
        /// </summary>
        public string Name { get; private set; }

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
                                Console.WriteLine($"[*] 当前交互标识 {api.SessionID}");
                                using (var session = new it.Session(api.SessionID)) {
                                    // 自动申请新的交互标识
                                    if (!session.Enable) {
                                        session.CreateNewSessionID();
                                        api.SessionID = session.SessionID;
                                        Console.WriteLine($"[+] 生成了一个全新的交互标识 {api.SessionID}");
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
                                Console.WriteLine($"[*] 当前交互标识 {api.SessionID}");
                                using (var session = new it.Session(api.SessionID)) {
                                    // 自动申请新的交互标识
                                    if (!session.Enable) {
                                        session.CreateNewSessionID();
                                        api.SessionID = session.SessionID;
                                        Console.WriteLine($"[+] 生成了一个全新的交互标识 {api.SessionID}");
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
            // Console.WriteLine($"[*] 包 {this.Name} 接受 {request.Method} {path} ...");
            // 执行事件
            switch (request.Method) {
                case "POST":
                    // 遍历所有注册接口
                    foreach (var info in _post) {
                        if (path == info.Route) {
                            return ExecuteMethod(context, info);
                        }
                    }
                    break;
                case "GET":
                    // 遍历所有注册接口
                    foreach (var info in _get) {
                        if (path == info.Route) {
                            return ExecuteMethod(context, info);
                        }
                    }
                    // 判断是否存在静态文件
                    string filePath = $"{this.RootPath}{path.Replace('/', it.SplitChar)}";
                    // Console.WriteLine($"[*] 查找静态文件 {filePath} ...");
                    if (System.IO.File.Exists(filePath)) {
                        // Console.WriteLine($"[*] 输出文件 {filePath} ...");
                        string ext = System.IO.Path.GetExtension(filePath).ToLower();
                        if (_mimes.ContainsKey(ext)) {
                            // Console.WriteLine($"[*] 输出mime {_mimes[ext]} ...");
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

            // Console.WriteLine($"    正在注册类库 {path} ...");

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
                                                    Console.WriteLine($"[@] POST /{this.Name}{routeNew} ...");
                                                    _post.Add(methodInfo);
                                                    break;
                                                case dpz3.Modular.ModularTypes.Get:
                                                    // 添加一条GET接口信息
                                                    Console.WriteLine($"[@] GET /{this.Name}{routeNew} ...");
                                                    _get.Add(methodInfo);
                                                    // 处理特殊的首页
                                                    if (routeNew.EndsWith("/index")) {
                                                        routeNew = routeNew.Substring(0, routeNew.Length - 5);
                                                        // 添加一条GET接口信息
                                                        Console.WriteLine($"[@] GET /{this.Name}{routeNew} ...");
                                                        _get.Add(new ModularMethodInfo() {
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

        public ModularPackage(dpz3.KeyValues<string> mimes, string folderPackage, string packageName, string packageInstall) {
            // 初始化接口列表信息
            _post = new List<ModularMethodInfo>();
            _get = new List<ModularMethodInfo>();
            _mimes = mimes;
            // 读取包信息
            this.Name = packageName;
            string folderInstall = $"{folderPackage}{it.SplitChar}{packageName}{it.SplitChar}{packageInstall}";
            this.RootPath = $"{folderInstall}{it.SplitChar}wwwroot";
            Console.WriteLine($"[*] 读取包版本 {packageName} 安装版本:{packageInstall}");
            // 判断依赖有效性
            string pathCfg = $"{folderInstall}{it.SplitChar}modular.json";
            // Console.WriteLine($"    读取包配置文件 {pathCfg} ...");
            bool isEnable = false;
            int plmVersion = 0;
            if (System.IO.File.Exists(pathCfg)) {
                string szJson = dpz3.File.UTF8File.ReadAllText(pathCfg);
                using (var json = dpz3.Json.Parser.ParseJson(szJson)) {
                    string jsonName = json.Str["name"];
                    string jsonVersion = json.Str["version"];
                    string jsonDescription = json.Str["description"];
                    var jsonDepends = json.Arr["depend"];
                    // Console.WriteLine($"    获取包 {packageName} 信息");
                    Console.WriteLine($"    名称:{jsonName}");
                    Console.WriteLine($"    版本:{jsonVersion}");
                    Console.WriteLine($"    描述:{jsonDescription}");
                    for (int i = 0; i < jsonDepends.Count; i++) {
                        var depend = jsonDepends.Obj[i];
                        string dependPlatform = depend.Str["platform"];
                        int dependVersion = depend.Int["version"];
                        Console.WriteLine($"    依赖 => {dependPlatform}:{dependVersion}");
                        if (dependPlatform == Platform_Name) {
                            plmVersion = dependVersion;
                        }
                    }
                }
            }
            if (plmVersion < Platform_Version) {
                Console.WriteLine($"[-] 包 {packageName} 依赖关系不符合: 此包适用于旧版本主程序，请更新包或联系开发者进行更新");
            } else if (plmVersion < Platform_Version) {
                Console.WriteLine($"[-] 包 {packageName} 依赖关系不符合: 此包适用于更新版本的主程序，请升级主程序");
            } else {
                isEnable = true;
            }
            if (isEnable) {
                // 注册控制器
                string pathDll = $"{folderInstall}{it.SplitChar}controller.dll";
                if (System.IO.File.Exists(pathDll)) {
                    Console.WriteLine($"    包 {packageName} 控制器激活中 ...");
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
                Console.WriteLine($"[+] 创建目录 {pathNew} ...");
                if (!System.IO.Directory.Exists(pathNew)) System.IO.Directory.CreateDirectory(pathNew);
                CopyFolder(dir, pathNew);
            }

            // 复制文件
            string[] files = System.IO.Directory.GetFiles(pathSource);
            foreach (var file in files) {
                string name = System.IO.Path.GetFileName(file);
                string pathNew = $"{pathTarget}{it.SplitChar}{name}";
                Console.WriteLine($"[+] 拷贝文件 {pathNew} ...");
                System.IO.File.Copy(file, $"{pathTarget}{it.SplitChar}{name}", true);
            }
        }

        // 安装所有的包
        private void InstallPacks() {

            Console.WriteLine("[*] 初始化包管理 ...");

            string folderRoot = $"{it.ExecPath}wwwroot";
            string folderDown = $"{it.ExecPath}downloads";
            string folderPackage = $"{it.ExecPath}packages";
            if (!System.IO.Directory.Exists(folderRoot)) System.IO.Directory.CreateDirectory(folderRoot);
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

        public Task Invoke(HttpContext httpContext) {
            // 获取申请器
            var request = httpContext.Request;
            // 读取访问地址
            string path = request.Path.ToString().ToLower();
            Console.WriteLine($"[*] {request.Method} {path}");
            // 遍历所有包
            foreach (var package in _packages) {
                if (path.StartsWith($"/{package.Name}/")) {
                    return package.Invoke(httpContext, path.Substring(package.Name.Length + 1));
                }
            }
            return _next(httpContext);
        }

    }

    public static class ModularMiddlewareExtensions {
        public static IApplicationBuilder UseCrossDomainMiddleware(this IApplicationBuilder builder) {
            return builder.UseMiddleware<ModularMiddleware>();
        }
    }

}
