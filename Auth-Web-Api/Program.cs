using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NSwag.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["AzureAd:Authority"];
        options.Audience = builder.Configuration["AzureAd:Audience"];
    });

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
// Configuración de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://login.microsoftonline.com/" + builder.Configuration["AzureAd:TenantId"] + "/oauth2/v2.0/authorize"),
                TokenUrl = new Uri("https://login.microsoftonline.com/" + builder.Configuration["AzureAd:TenantId"] + "/oauth2/v2.0/authorize"),
                Scopes = new Dictionary<string, string>
                {
                    { builder.Configuration["AzureAd:Scopes"] ?? "User.Read", "User.Read" }
                }
            }
        }
    });

    c.OperationFilter<AddAuthorizeHeaderOperationFilter>(); // Utiliza la clase AddAuthorizeHeaderOperationFilter
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.OAuthClientId(builder.Configuration["AzureAd:ClientId"]);
        c.OAuthAppName("My API V1");
        c.OAuthUsePkce();
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();