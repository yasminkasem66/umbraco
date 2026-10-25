
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

// Additional allowed origins (e.g. a deployed frontend) can be added without a code change via
// the "AngularApp:AdditionalOrigins" config array — see appsettings.Production.json.
string[] additionalOrigins = builder.Configuration.GetSection("AngularApp:AdditionalOrigins").Get<string[]>() ?? [];
string[] corsOrigins = ["http://localhost:4200", .. additionalOrigins];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularApp", policy =>
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Contact-form validation errors (contracts/contact-submission.md) use ContactController's own
// { status, errors } shape instead of the framework's default ProblemDetails 400 body.
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});


WebApplication app = builder.Build();

app.UseCors("AngularApp");

await app.BootUmbracoAsync();


app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
