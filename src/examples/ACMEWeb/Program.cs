using ACMESharp.Enrollment;
using ACMEWeb;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();
builder.Services.AddMvc();
builder.Services.AddAcmeHandler(builder.Configuration);
builder.Services.AddTransient<AcmeRevokeService>();
//builder.WebHost.UseUrls("http://*:80");

var app = builder.Build();

app.MapGet("/", () => "OK");

app.MapGet(".well-known/acme-challenge/{token}", (string token, IChallengeProvider challengeProvider) =>
{
    return (challengeProvider as HttpChallaneProvider).ValidateChallenge(token);
});

app.MapGet("/revoke", async () =>
{
    await app.Services.GetService<AcmeRevokeService>().RevokeCertificate();
    return "Revoked";
});

app.Run("http://*:80");
