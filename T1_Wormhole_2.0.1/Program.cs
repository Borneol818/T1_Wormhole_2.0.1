using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using T1_Wormhole_2._0._1;
using T1_Wormhole_2._0._1.Models.Database;
using T1_Wormhole_2._0._1.LoginScripts;
using Microsoft.AspNetCore.OData;
using Hangfire;
using Google;
using System.Security.Claims;
using T1_Wormhole_2._0._1.Filter;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDataProtection();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddControllersWithViews().AddOData(options => {
    options.Select()//挑欄位
           .Filter()//篩選
           .OrderBy()//排序
           .Expand()//關聯查詢
           .SetMaxTop(100)//最多100筆
           .Count();//計算數量
}).AddCookieTempDataProvider();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/Account/Login";
    });



var conStr = builder.Configuration.GetConnectionString("WormHole");

// 加入 Hangfire 服務
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(conStr));
// 加入 Hangfire Server
builder.Services.AddHangfireServer();

//啟用 NetTopologySuite
builder.Services.AddDbContext<WormHoleContext>(x =>
    x.UseSqlServer(conStr, sqlOptions => sqlOptions.UseNetTopologySuite()));
//這裡寫一個判定用的方法並存入在這裡new的變數名稱，用來當作登入後的認證跟各項頁面功能的全域變數

// 添加 Session
builder.Services.AddDistributedMemoryCache(); // 使用記憶體快取儲存 Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Session 有效期 20 分鐘
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // 確保 Session Cookie 符合 GDPR
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
// 啟用 Hangfire Dashboard
app.UseHangfireDashboard("/hangfire",new DashboardOptions 
{
    Authorization = new[] {new ClaimBaseHangfireDashboardAuthorizationFilter("Admin") }
});

app.UseRouting();
app.UseSession();
app.UseAuthentication();

app.UseAuthorization();



app.Use(async (context, next) =>
{
    if (context.User.Identity.IsAuthenticated && context.User.FindFirst(ClaimTypes.Role)?.Value == "User")
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            var dbContext = context.RequestServices.GetRequiredService<WormHoleContext>();
            var rewardService = context.RequestServices.GetRequiredService<IUserService>();
            var ConvertUserId = Convert.ToInt32(userId);

            // 檢查是否為新會話
            var sessionKey = $"Visit_{userId}";
            if (!context.Session.TryGetValue(sessionKey, out _))
            {
                // 嘗試發放每日獎勵
                var GetReward = await rewardService.DailyRewardAsync(ConvertUserId);
                if (GetReward) 
                {
                    context.Items["RewardMessage"] = "已獲得每日登入獎勵 - 2枚金幣";
                }
                // 新會話，記錄到 LoginRecord 表
                else
                {
                    var LoginRecord = new LoginRecord
                    {
                        UserId = ConvertUserId,
                        Time = DateTime.UtcNow
                    };
                    if (dbContext != null)
                    {
                        dbContext.LoginRecords.Add(LoginRecord);
                        await dbContext.SaveChangesAsync();
                    }
                }
                // 設置 Session 標記
                context.Session.SetString(sessionKey, "Active");

                
            }
        }
    }
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
