using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ModularCore {
    public class Startup {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            // 使用自定义的模块化中间件
            app.UseModular();

            // 开启静态文件支持并单独处理非缓存文件
            dpz3.KeyValues<string> noneCache = new dpz3.KeyValues<string>();
            noneCache["json"] = "text/plain";
            noneCache["html"] = "text/html";

            // 设置header
            dpz3.KeyValues<string> noneCacheHeaders = new dpz3.KeyValues<string>();
            noneCacheHeaders["Pragma"] = "No-Cache";
            noneCacheHeaders["Cache-Control"] = "No-Cache";
            noneCacheHeaders["Expires"] = "0";

            // 使用静态文件
            app.UseStaticFiles(new StaticFileOptions() {
                OnPrepareResponse = context => {
                    // 判断扩展名
                    string ext = System.IO.Path.GetExtension(context.Context.Request.Path).Substring(1).ToLower();
                    foreach (var item in noneCache) {
                        if (item.Key == ext) {
                            // 设置内容类型
                            context.Context.Response.ContentType = item.Value;
                            foreach (var header in noneCacheHeaders) {
                                if (context.Context.Response.Headers.ContainsKey(header.Key)) {
                                    context.Context.Response.Headers[header.Key] = header.Value;
                                } else {
                                    context.Context.Response.Headers.Add(header.Key, header.Value);
                                }
                            }
                            break;
                        }
                    }
                }
            });

            // 启用路由
            app.UseRouting();

            app.UseEndpoints(endpoints => {
                endpoints.MapGet("/", async context => {
                    string html;
                    string pathIndex = $"{it.ExecPath}wwwroot/index.html";
                    if (System.IO.File.Exists(pathIndex)) {
                        html = dpz3.File.UTF8File.ReadAllText(pathIndex);
                    } else {
                        html = "<html>" +
                            "<head>" +
                            "<meta charset=\"utf-8\" />" +
                            "<title>模块化网站</title>" +
                            "</head>" +
                            "<body>" +
                            "<div>尚未指定index.html页面</div>" +
                            "</body>" +
                            "</html>";
                    }
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync(html);
                });
                //endpoints.MapGet("/", async context => {
                //    await context.Response.WriteAsync("Hello World!");
                //});
            });
        }
    }
}
