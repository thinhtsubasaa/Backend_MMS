using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ERP.Controllers;
using ERP.Data;
using ERP.Filters;
using ERP.Helpers;
using ERP.HubConfig;
using ERP.Infrastructure;
using ERP.Models;
using ERP.UOW;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;
using ThacoLibs;
using static ERP.Commons;
using static ERP.Data.MyDbContext;
using static ERP.Controllers.AdsunController;
using Microsoft.AspNetCore.Http.Connections;

namespace ERP
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddSingleton<IConfiguration>(Configuration);
      services.AddSingleton(Configuration);
      services.AddScoped<MyTypedClient>();
      services.AddHttpClient("MyHttpClient");
      services.AddScoped<DownloadImage>();
      services.AddTransient<IUnitofWork, UnitofWork>();
      // services.AddHostedService<VehicleDataUpdateService>();
      services.AddScoped<VehicleService>();
      services.AddDbContext<MyDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
      services.AddIdentity<ApplicationUser, ApplicationRole>().AddEntityFrameworkStores<MyDbContext>().AddDefaultTokenProviders();
      services.AddCors(options => options.AddPolicy("CorsApi",
  builder =>
  {
    builder.AllowAnyHeader()
           .AllowAnyMethod()
           .SetIsOriginAllowed((host) => true)
           .AllowCredentials();
  }));

      services.AddSignalR();
      /*services.AddScoped<PhieuHoTroNguoiDungController>();*/
      // configure strongly typed settings objects
      var appSettingsSection = Configuration.GetSection("AppSettings");
      services.Configure<AppSettings>(appSettingsSection);

      // configure jwt authentication
      var appSettings = appSettingsSection.Get<AppSettings>();
      var key = Encoding.ASCII.GetBytes(appSettings.Secret);

      services.AddAuthentication(x =>
      {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      })
      .AddJwtBearer(x =>
      {


        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(key),
          ValidateIssuer = false,
          ValidateAudience = false,
          ValidateLifetime = true,
          ClockSkew = TimeSpan.Zero
        };
        x.Events = new JwtBearerEvents
        {
          OnMessageReceived = context =>
                       {
                         var accessToken = context.Request.Query["access_token"];
                         // If the request is for our hub...
                         var path = context.HttpContext.Request.Path;
                         if (!string.IsNullOrEmpty(accessToken) &&
                              (path.StartsWithSegments("/dashboard")))
                         {
                           // Read the token out of the query string
                           context.Token = accessToken;
                         }
                         return Task.CompletedTask;
                       }
        };
      });
      /*services.AddSignalR().AddJsonProtocol(options => options.PayloadSerializerOptions.PropertyNamingPolicy = null);*/
      services.AddControllersWithViews(options =>
      {
        options.Filters.Add(typeof(ApiKeyActionFilter));
      });
      services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
      services.AddMvc().AddControllersAsServices();
      // Register the Swagger generator, defining 1 or more Swagger documents
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
        c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
        {
          Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",
          Type = SecuritySchemeType.Http,
          BearerFormat = "JWT",
          Name = "JWT Authentication",
          In = ParameterLocation.Header,
          Scheme = "bearer"
        });
        c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
        {
          Description = "Enter your Api Key below:",
          Name = "ApiKey",
          In = ParameterLocation.Header,
          Type = SecuritySchemeType.ApiKey
        });
        // add auth header for [Authorize] endpoints
        c.OperationFilter<AddAuthHeaderOperationFilter>();
      });
      services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
      // Enable middleware to serve generated Swagger as a JSON endpoint.
      app.UseSwagger();
      // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
      // specifying the Swagger JSON endpoint.
      app.UseSwaggerUI(c =>
{
  c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
  c.RoutePrefix = string.Empty;
});
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseStatusCodePages();
      }
      // MyIdentityDataInitializer.SeedData(userManager, roleManager).Wait();
      app.UseHttpsRedirection();
      app.UseRouting();
      app.UseCors("CorsApi");
      app.UseExceptionHandler(appBuilder =>
            {
              appBuilder.Use(async (context, next) =>
              {
                var error = context.Features[typeof(IExceptionHandlerFeature)] as IExceptionHandlerFeature;

                if (error != null && error.Error is SecurityTokenExpiredException)
                {
                  context.Response.StatusCode = 401;
                  context.Response.ContentType = "application/json";

                  await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                  {
                    State = "Unauthorized",
                    Msg = "token expired"
                  }));
                }

                else if (error != null && error.Error != null)
                {
                  context.Response.StatusCode = 500;
                  context.Response.ContentType = "application/json";
                  await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                  {
                    State = "Internal Server Error",
                    Msg = error.Error.Message
                  }));
                }
                //when no error, do next.
                else await next();
              });
            });
      app.UseAuthentication();
      app.UseAuthorization();
      app.UseStaticFiles(new StaticFileOptions
      {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "Uploads")),
        RequestPath = "/Uploads",
        OnPrepareResponse = ctx =>
               {
                 if (ctx.Context.Request.Path.StartsWithSegments("/Uploads"))
                 {
                   ctx.Context.Response.Headers.Add("Cache-Control", "no-store");

                   //  if (!ctx.Context.User.Identity.IsAuthenticated)
                   //  {
                   //    // respond HTTP 401 Unauthorized with empty body.
                   //    ctx.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                   //    ctx.Context.Response.ContentLength = 0;
                   //    ctx.Context.Response.Body = Stream.Null;

                   //    // - or, redirect to another page. -
                   //    // ctx.Context.Response.Redirect("/");
                   //  }
                 }
               }
      });
      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
        endpoints.MapHub<DashboardHub>("/dashboard");
        // endpoints.MapHub<XeRaCongHub>("/xeRaCongHub", options =>
        // {
        //   options.Transports = HttpTransportType.WebSockets |
        //                         HttpTransportType.ServerSentEvents |
        //                         HttpTransportType.LongPolling;
        // });
      });

      /*            app.UseEndpoints(endpoints =>
                  {
                      endpoints.MapHub<PhieuHoTroNguoiDungController.ChatHub>("/chatHub");
                      endpoints.MapControllers();
                  });*/
    }
  }
  public class AddAuthHeaderOperationFilter : IOperationFilter
  {
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
      if (operation.Security == null)
        operation.Security = new List<OpenApiSecurityRequirement>();
      var scheme = new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearer" } };
      var schemekey = new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" } };
      operation.Security.Add(new OpenApiSecurityRequirement
      {
        [scheme] = new List<string>(),
        [schemekey] = new List<string>()
      });
    }
  }
  public class NameUserIdProvider : IUserIdProvider
  {
    public string GetUserId(HubConnectionContext connection)
    {
      return connection.User?.Identity?.Name;
    }
  }
}
