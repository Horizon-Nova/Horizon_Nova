using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YiSha.Util.Model;
using YiSha.Util.Extension;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using YiSha.Util;
using Microsoft.Extensions.Hosting;

namespace YiSha.Admin.Web.Controllers
{
    /// <summary>
    /// 全局異常過滤器
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter, IAsyncExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            LogHelper.Error(context.Exception);
            if (context.HttpContext.Request.IsAjaxRequest())
            {
                TData obj = new TData();
                obj.Message = context.Exception.GetOriginalException().Message;
                if (string.IsNullOrEmpty(obj.Message))
                {
                    obj.Message = "抱歉，系统錯誤，請联系管理員！";
                }
                context.Result = new JsonResult(obj);
                context.ExceptionHandled = true;
            }
            else
            {
                string errorMessage = context.Exception.GetOriginalException().Message;
                context.Result = new RedirectResult("~/Home/Error?message=" + HttpUtility.UrlEncode(errorMessage));
                context.ExceptionHandled = true;
            }
        }

        public Task OnExceptionAsync(ExceptionContext context)
        {
            if (GlobalContext.HostingEnvironment.IsProduction())
            {
                OnException(context);
            }
            return Task.CompletedTask;
        }
    }
}