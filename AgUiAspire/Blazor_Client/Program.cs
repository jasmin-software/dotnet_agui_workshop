using Blazor_Client.Components;
using Blazor_Client.Services;

var builder = WebApplication.CreateBuilder(args);

string serverUrl = Environment.GetEnvironmentVariable("AGUI_SERVER_URL") ?? "http://localhost:5000";
builder.Services.AddScoped(sp => {
    var http = new HttpClient {
        BaseAddress = new Uri(serverUrl),
        Timeout = TimeSpan.FromSeconds(60)
    };
    return http;
});
builder.Services.AddScoped<IAGUIAgentService, AGUIAgentService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
