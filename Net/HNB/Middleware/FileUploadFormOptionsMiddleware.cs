using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace HNB.Middleware;

/// <summary>
/// 檔案上傳表單選項中介軟體，用於在上傳路由上提前設定 FormOptions
/// 確保 ValueCountLimit 在模型綁定之前就生效
/// </summary>
public class FileUploadFormOptionsMiddleware
{
    private readonly RequestDelegate _next;

    public FileUploadFormOptionsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 只針對檔案上傳路由設定 FormOptions
        if (context.Request.Path.StartsWithSegments("/Backoffice/FileManager/SubmitUpload", StringComparison.OrdinalIgnoreCase) &&
            context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            var formOptions = new FormOptions
            {
                ValueCountLimit = int.MaxValue,
                ValueLengthLimit = int.MaxValue,
                MultipartBodyLengthLimit = 5368709120, // 5GB
                MultipartHeadersLengthLimit = int.MaxValue,
                MultipartBoundaryLengthLimit = int.MaxValue
            };

            var formFeature = new FormFeature(context.Request, formOptions);
            context.Features.Set<IFormFeature>(formFeature);
        }

        await _next(context);
    }
}

