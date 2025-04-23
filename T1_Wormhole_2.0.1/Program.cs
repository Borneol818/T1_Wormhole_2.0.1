using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using T1_Wormhole_2._0._1;
using T1_Wormhole_2._0._1.Models.Database;
using T1_Wormhole_2._0._1.LoginScripts;
using Microsoft.AspNetCore.OData;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddControllersWithViews().AddOData(options => {
    options.Select()//�D���
           .Filter()//�z��
           .OrderBy()//�Ƨ�
           .Expand()//���p�d��
           .SetMaxTop(100)//�̦h100��
           .Count();//�p��ƶq
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/Account/Login";
    });


var conStr = builder.Configuration.GetConnectionString("WormHole");

builder.Services.AddDbContext<WormHoleContext>(x => x.UseSqlServer(conStr));


// �K�[ Hangfire �A��
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(conStr));

builder.Services.AddHangfireServer();

//�o�̼g�@�ӧP�w�Ϊ���k�æs�J�b�o��new���ܼƦW�١A�Ψӷ�@�n�J�᪺�{�Ҹ�U�������\�઺�����ܼ�
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

app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();

// �ҥ� Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
