using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();

builder.Services.AddAuthentication("cookies")
    .AddCookie("cookies", options =>
    {
        options.LoginPath = "/login.html";
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/chatHub");

// ✅ Servir index.html al entrar a "/"
app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/index.html");
});

// ✅ Login
app.MapPost("/login", async (HttpContext ctx) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var username = form["username"];
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, username!)
    };
    var identity = new ClaimsIdentity(claims, "cookies");
    var principal = new ClaimsPrincipal(identity);

    await ctx.SignInAsync("cookies", principal);
    ctx.Response.Redirect("/");
});

app.Run();
