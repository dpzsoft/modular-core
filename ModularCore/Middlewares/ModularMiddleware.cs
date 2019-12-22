using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using dpz3;
using dpz3.Modular;

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
        /// 是否拥有交互管理器
        /// </summary>
        public bool HasSession { get; set; }
    }

    public class ModularMiddleware {

        private readonly RequestDelegate _next;
        private List<ModularMethodInfo> _post;
        private List<ModularMethodInfo> _get;

        // 注册类库
        private void RegLibrary(string path) {
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
                            case ModularTypes.SessionApi:
                                // 读取路由地址定义
                                string route = modular.Route.Replace("{ControllerName}", tp.Name);
                                // 遍历所有的函数定义
                                var methods = tp.GetMethods();
                                foreach (var method in methods) {
                                    // 遍历函数特性，判断函数是否满足条件
                                    var methodAttrs = method.GetCustomAttributes();
                                    foreach (var methodAttr in methodAttrs) {
                                        if (methodAttr.GetType().FullName == "dpz3.Modular.ModularAttribute") {
                                            // 重建模块化特性
                                            dpz3.Modular.ModularAttribute methodModular = (dpz3.Modular.ModularAttribute)methodAttr;
                                            switch (methodModular.ModularType) {
                                                case dpz3.Modular.ModularTypes.Post:
                                                    // 添加一条POST接口信息
                                                    _post.Add(new ModularMethodInfo() {
                                                        Method = method,
                                                        Assembly = assembly,
                                                        Type = tp,
                                                        Route = $"{route}/{methodModular.Route}",
                                                        HasSession = modular.ModularType == ModularTypes.SessionApi
                                                    });
                                                    break;
                                                case dpz3.Modular.ModularTypes.Get:
                                                    // 添加一条GET接口信息
                                                    _get.Add(new ModularMethodInfo() {
                                                        Method = method,
                                                        Assembly = assembly,
                                                        Type = tp,
                                                        Route = $"{route}/{methodModular.Route}",
                                                        HasSession = modular.ModularType == ModularTypes.SessionApi
                                                    }); ;
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

        public ModularMiddleware(RequestDelegate next) {
            _next = next;
            _post = new List<ModularMethodInfo>();
            _get = new List<ModularMethodInfo>();
            string path = $"{it.ExecPath}lib{it.SplitChar}ClassDemo.dll";
            RegLibrary(path);
        }

        private Task ExecuteMethod(HttpContext httpContext, ModularMethodInfo info) {
            try {
                string returnType = info.Method.ReturnType.FullName;
                switch (returnType) {
                    case "System.String":
                        // 处理返回为字符串的函数定义
                        if (info.HasSession) {
                            JttpSessionControllerBase api = (JttpSessionControllerBase)info.Assembly.CreateInstance(info.Type.FullName); ;
                            // 执行初始化调用
                            string res = api.Initialize(httpContext);
                            using (it.Session session = new it.Session())
                                if (!res.IsNoneOrNull()) return httpContext.Response.WriteAsync(res);
                            // 调用主事件
                            res = (string)info.Method.Invoke(api, null);
                            dpz3.Modular.Result.Text text = new dpz3.Modular.Result.Text() { Content = res };
                            return api.Render(text);
                        } else {
                            ApiControllerBase api = (ApiControllerBase)info.Assembly.CreateInstance(info.Type.FullName); ;
                            // 执行初始化调用
                            string res = api.Initialize(httpContext);
                            if (!res.IsNoneOrNull()) return httpContext.Response.WriteAsync(res);
                            // 调用主事件
                            res = (string)info.Method.Invoke(api, null);
                            dpz3.Modular.Result.Text text = new dpz3.Modular.Result.Text() { Content = res };
                            return api.Render(text);
                        }
                    case "dpz3.Modular.IResult":
                        // 处理返回为标准接口的函数定义
                        if (info.HasSession) {
                            JttpSessionControllerBase api = (JttpSessionControllerBase)info.Assembly.CreateInstance(info.Type.FullName); ;
                            // 执行初始化调用
                            string res = api.Initialize(httpContext);
                            using (it.Session session = new it.Session())
                                if (!res.IsNoneOrNull()) return httpContext.Response.WriteAsync(res);
                            // 调用主事件
                            IResult result = (IResult)info.Method.Invoke(api, null);
                            return api.Render(result);
                        } else {
                            ApiControllerBase api = (ApiControllerBase)info.Assembly.CreateInstance(info.Type.FullName); ;
                            // 执行初始化调用
                            string res = api.Initialize(httpContext);
                            if (!res.IsNoneOrNull()) return httpContext.Response.WriteAsync(res);
                            // 调用主事件
                            IResult result = (IResult)info.Method.Invoke(api, null);
                            return api.Render(result);
                        }
                    default:
                        return httpContext.Response.WriteAsync($"尚未支持的返回类型 {returnType}");
                }
            } catch (Exception ex) {
                return httpContext.Response.WriteAsync(ex.ToString());
            }
        }

        public Task Invoke(HttpContext httpContext) {
            // 获取申请器
            var request = httpContext.Request;
            // 读取访问地址
            string path = request.Path;
            // 执行事件
            switch (request.Method) {
                case "POST":
                    foreach (var info in _post) {
                        if (path == info.Route) {
                            return ExecuteMethod(httpContext, info);
                        }
                    }
                    break;
                case "GET":
                    foreach (var info in _get) {
                        if (path == info.Route) {
                            return ExecuteMethod(httpContext, info);
                        }
                    }
                    break;
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
