using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModularCore {
    public static class MiddlewareExtensions {
        public static IApplicationBuilder UseModular(this IApplicationBuilder builder) {
            return builder.UseMiddleware<Middlewares.ModularMiddleware>();
        }
    }
}
