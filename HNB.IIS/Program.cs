using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// 配置轉發標頭（確保 IP 正確傳遞給後端）
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// 添加 YARP 反向代理
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// 添加健康檢查
builder.Services.AddHealthChecks();

var app = builder.Build();

// 使用轉發標頭
app.UseForwardedHeaders();

// 健康檢查端點 (Railway 需要)
app.MapHealthChecks("/health");

// YARP 反向代理
app.MapReverseProxy();

app.Run();

