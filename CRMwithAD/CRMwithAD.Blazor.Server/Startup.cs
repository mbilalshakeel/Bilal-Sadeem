using CRMwitAD.Shared.Services;
using CRMwithAD.Blazor.Server.Configurations;
using CRMwithAD.Blazor.Server.Models.InternalModels;
using CRMwithAD.Blazor.Server.Services;
using CRMwithAD.Module.BusinessObjects.BillingManagement;
using CRMwithAD.Module.BusinessObjects.Helpdesk_Tickets;
using CRMwithAD.Module.BusinessObjects.SubscriptionsManagement;
using CRMwithAD.Module.Services;
using CRMwithAD.WebApi.JWT;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.Authentication.ClientServer;
using DevExpress.ExpressApp.WebApi.Services;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.OData;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

namespace CRMwithAD.Blazor.Server;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        AppSettingsProvider.Initialize(Configuration);

        services.AddSingleton(typeof(Microsoft.AspNetCore.SignalR.HubConnectionHandler<>), typeof(ProxyHubConnectionHandler<>));

        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddHttpContextAccessor();
        services.AddScoped<IAuthenticationTokenProvider, JwtTokenProviderService>();
        services.AddScoped<CircuitHandler, CircuitHandlerProxy>();
        services.AddScoped<Logging>();

        services.Configure<ADAuthentication>(Configuration.GetSection("ADAuthentication"));
        services.AddScoped<ADAuthService>();

        services.AddXaf(Configuration, builder =>
        {
            builder.UseApplication<CRMwithADBlazorApplication>();

            builder.AddXafWebApi(webApiBuilder =>
            {
                webApiBuilder.AddXpoServices();

                webApiBuilder.ConfigureOptions(options =>
                {
                    // Make your business objects available in the Web API and generate the GET, POST, PUT, and DELETE HTTP methods for it.
                    // options.BusinessObject<YourBusinessObject>();
                    options.BusinessObject<Ticket>();
                });
            });

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
            });

            builder.Modules
                .AddAuditTrailXpo()
                .AddCloning()
                .AddConditionalAppearance()
                .AddDashboards(options =>
                {
                    options.DashboardDataType = typeof(DevExpress.Persistent.BaseImpl.DashboardData);
                })
                .AddFileAttachments()
                .AddNotifications()
                .AddOffice()
                .AddReports(options =>
                {
                    options.EnableInplaceReports = true;
                    options.ReportDataType = typeof(DevExpress.Persistent.BaseImpl.ReportDataV2);
                    options.ReportStoreMode = DevExpress.ExpressApp.ReportsV2.ReportStoreModes.XML;
                })
                .AddScheduler()
                .AddStateMachine(options =>
                {
                    options.StateMachineStorageType = typeof(DevExpress.ExpressApp.StateMachine.Xpo.XpoStateMachine);
                })
                .AddValidation(options =>
                {
                    options.AllowValidationDetailsAccess = false;
                })
                .AddViewVariants()
                .Add<CRMwithAD.Module.CRMwithADModule>()
                .Add<CRMwithADBlazorModule>();
            builder.ObjectSpaceProviders
                .AddSecuredXpo((serviceProvider, options) =>
                {
                    string connectionString = null;
                    if (Configuration.GetConnectionString("ConnectionString") != null)
                    {
                        connectionString = Configuration.GetConnectionString("ConnectionString");
                    }
#if EASYTEST
                    if(Configuration.GetConnectionString("EasyTestConnectionString") != null) {
                        connectionString = Configuration.GetConnectionString("EasyTestConnectionString");
                    }
#endif
                    ArgumentNullException.ThrowIfNull(connectionString);
                    options.ConnectionString = connectionString;
                    options.ThreadSafe = true;
                    options.UseSharedDataStoreProvider = true;
                })
                .AddNonPersistent();
            builder.Security
                .UseIntegratedMode(options =>
                {
                    options.Lockout.Enabled = true;

                    options.RoleType = typeof(PermissionPolicyRole);
                    // ApplicationUser descends from PermissionPolicyUser and supports the OAuth authentication. For more information, refer to the following topic: https://docs.devexpress.com/eXpressAppFramework/402197
                    // If your application uses PermissionPolicyUser or a custom user type, set the UserType property as follows:
                    options.UserType = typeof(CRMwithAD.Module.BusinessObjects.ApplicationUser);
                    // ApplicationUserLoginInfo is only necessary for applications that use the ApplicationUser user type.
                    // If you use PermissionPolicyUser or a custom user type, comment out the following line:
                    options.UserLoginInfoType = typeof(CRMwithAD.Module.BusinessObjects.ApplicationUserLoginInfo);
                    options.UseXpoPermissionsCaching();
                    options.Events.OnSecurityStrategyCreated += securityStrategy =>
                    {
                        // Use the 'PermissionsReloadMode.NoCache' option to load the most recent permissions from the database once
                        // for every Session instance when secured data is accessed through this instance for the first time.
                        // Use the 'PermissionsReloadMode.CacheOnFirstAccess' option to reduce the number of database queries.
                        // In this case, permission requests are loaded and cached when secured data is accessed for the first time
                        // and used until the current user logs out. 
                        // See the following article for more details: https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Security.SecurityStrategy.PermissionsReloadMode.
                        ((SecurityStrategy)securityStrategy).PermissionsReloadMode = PermissionsReloadMode.NoCache;
                    };
                })
                .AddPasswordAuthentication(options =>
                {
                    options.IsSupportChangePassword = true;
                })
                .AddWindowsAuthentication(options =>
                {
                    options.CreateUserAutomatically();
                });
            builder.ObjectSpaceProviders.Events.OnObjectSpaceCreated = context =>
            {
                var nonPersistentObjectSpace = context.ObjectSpace as NonPersistentObjectSpace;
                if (nonPersistentObjectSpace != null)
                {
                    nonPersistentObjectSpace.ObjectsGetting += NonPersistentObjectSpace_ObjectsGetting;
                }
            };
        });

        var authentication = services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        });
        authentication.AddCookie(options =>
        {
            options.LoginPath = "/LoginPage";
        });
        authentication.AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Authentication:Jwt:IssuerSigningKey"]))
            };
        });
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(
                JwtBearerDefaults.AuthenticationScheme,
                "Windows"
                )
                    .RequireAuthenticatedUser()
                    .RequireXafAuthentication()
                    .Build();
        });
        services.AddSingleton<LicenseValidator>();

        // Register IDataLayer for XPO
        services.AddSingleton<IDataLayer>(provider =>
        {
            // Get connection string from configuration
            var configuration = provider.GetRequiredService<IConfiguration>();
            string connectionString = configuration.GetConnectionString("ConnectionString");
            ArgumentNullException.ThrowIfNull(connectionString);

            // Create XPO dictionary and connection provider
            var dict = new DevExpress.Xpo.Metadata.ReflectionDictionary();
            var connectionProvider = XpoDefault.GetConnectionProvider(connectionString, AutoCreateOption.SchemaAlreadyExists);

            // Optionally, load schema for your persistent classes
            dict.GetDataStoreSchema(typeof(CRMwithAD.Module.BusinessObjects.LicenseManagement.License).Assembly);

            return new ThreadSafeDataLayer(dict, connectionProvider);
        });

        services
            .AddControllers()
            .AddOData((options, serviceProvider) =>
            {
                options
                    .AddRouteComponents("api/odata", new EdmModelBuilder(serviceProvider).GetEdmModel(), Microsoft.OData.ODataVersion.V401, _routeServices =>
                    {
                        _routeServices.ConfigureXafWebApiServices();
                    })
                    .EnableQueryFeatures(100);
            });

        services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CRMwithAD API",
                Version = "v1",
                Description = @"Use AddXafWebApi(options) in the CRMwithAD.Blazor.Server\Startup.cs file to make Business Objects available in the Web API."
            });
            c.AddSecurityDefinition("JWT", new OpenApiSecurityScheme()
            {
                Type = SecuritySchemeType.Http,
                Name = "Bearer",
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
                {
                    new OpenApiSecurityScheme() {
                        Reference = new OpenApiReference() {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "JWT"
                        }
                    },
                    new string[0]
                },
            });
        });

        services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o =>
        {
            //The code below specifies that the naming of properties in an object serialized to JSON must always exactly match
            //the property names within the corresponding CLR type so that the property names are displayed correctly in the Swagger UI.
            //XPO is case-sensitive and requires this setting so that the example request data displayed by Swagger is always valid.
            //Comment this code out to revert to the default behavior.
            //See the following article for more information: https://learn.microsoft.com/en-us/dotnet/api/system.text.json.jsonserializeroptions.propertynamingpolicy
            o.JsonSerializerOptions.PropertyNamingPolicy = null;
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRMwithAD WebApi v1");
            });
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. To change this for production scenarios, see: https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRMwithAD WebApi v1");
        });

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
        });
        app.UseHttpsRedirection();
        app.UseRequestLocalization();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseXaf();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapXafEndpoints();
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
            endpoints.MapControllers();
        });
    }
    //private void NonPersistentObjectSpace_ObjectsGetting(object sender, ObjectsGettingEventArgs e)
    //{
    //    string CMPBaseURL = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("URls")["CMP"]!;
    //    string BillingMIddleWareAPIs = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("URls")["BillingMIddleWareAPIs"]!;
    //    string CMPUsername = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Credentials")["CMPUsername"]!;
    //    string CMPPassword = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Credentials")["CMPPassword"]!;

    //    if (e.ObjectType == typeof(SubscriptionPlan))
    //    {
    //        // Initialize the list to be returned
    //        BindingList<SubscriptionPlan> objects = new BindingList<SubscriptionPlan>();

    //        //CMP Authentication
    //        CMPAuthTokenResponse cMPAuthTokenResponse = new CMPAuthTokenResponse();
    //        CMPAuthTokenErrorResponse cMPAuthTokenErrorResponse = new CMPAuthTokenErrorResponse();
    //        Clouds clouds = new Clouds();
    //        Plans plans = new Plans();
    //        Pricing pricing = new Pricing();

    //        using (var cmpAuthTokenHandler = new HttpClientHandler())
    //        {
    //            cmpAuthTokenHandler.ServerCertificateCustomValidationCallback = (request, cert, chain, errors) => { return true; };
    //            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

    //            using (var cmpAuthTokenClient = new HttpClient(cmpAuthTokenHandler))
    //            {
    //                cmpAuthTokenClient.BaseAddress = new Uri(CMPBaseURL);
    //                var values = new Dictionary<string, string>
    //                     {
    //                         { "username", CMPUsername },
    //                         { "password", CMPPassword}
    //                     };
    //                var content = new FormUrlEncodedContent(values);

    //                using (var res = cmpAuthTokenClient.PostAsync("oauth/token?client_id=morph-api&grant_type=password&scope=write", content))
    //                {
    //                    if (res.Result.IsSuccessStatusCode)
    //                    {
    //                        string apiResponse = res.Result.Content.ReadAsStringAsync().Result;
    //                        cMPAuthTokenResponse = JsonSerializer.Deserialize<CMPAuthTokenResponse>(apiResponse)!;


    //                        //getting cloud
    //                        using (var handler = new HttpClientHandler())
    //                        {
    //                            handler.ServerCertificateCustomValidationCallback = (request, cert, chain, errors) => { return true; };
    //                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
    //                            using (var client = new HttpClient(handler))
    //                            {
    //                                client.BaseAddress = new Uri(CMPBaseURL);
    //                                client.DefaultRequestHeaders.Add("accept", "application/json");
    //                                client.DefaultRequestHeaders.Add("authorization", "bearer " + cMPAuthTokenResponse.access_token);
    //                                var response = client.GetAsync("api/zones?max=100");
    //                                //var response = await client.GetAsync("api/zone-types?name=Vmware");
    //                                if (response.Result.IsSuccessStatusCode)
    //                                {
    //                                    string cloudJson = response.Result.Content.ReadAsStringAsync().Result;
    //                                    clouds = JsonSerializer.Deserialize<Clouds>(cloudJson)!;

    //                                }
    //                                else
    //                                {
    //                                    string cloudJson = response.Result.Content.ReadAsStringAsync().Result;
    //                                    clouds = JsonSerializer.Deserialize<Clouds>(cloudJson)!;
    //                                }
    //                            }
    //                        }
    //                        //getting clouds

    //                        //getting plans

    //                        using (var handler = new HttpClientHandler())
    //                        {
    //                            handler.ServerCertificateCustomValidationCallback = (request, cert, chain, errors) => { return true; };
    //                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
    //                            using (var client = new HttpClient(handler))
    //                            {
    //                                client.BaseAddress = new Uri(CMPBaseURL);
    //                                client.DefaultRequestHeaders.Add("accept", "application/json");
    //                                client.DefaultRequestHeaders.Add("authorization", "bearer " + cMPAuthTokenResponse.access_token);
    //                                var response = client.GetAsync("api/service-plans?max=10000&offset=0&sort=name&direction=asc&includeZones=true");
    //                                //var response = await client.GetAsync("api/service-plans?&provisionTypeId=20");
    //                                if (response.Result.IsSuccessStatusCode)
    //                                {

    //                                    string planJson = response.Result.Content.ReadAsStringAsync().Result;
    //                                    plans = JsonSerializer.Deserialize<Plans>(planJson)!;

    //                                }
    //                            }
    //                        }
    //                        //getting plans

    //                        //getting pricing

    //                        using (var handler = new HttpClientHandler())
    //                        {
    //                            handler.ServerCertificateCustomValidationCallback = (request, cert, chain, errors) => { return true; };
    //                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
    //                            using (var client = new HttpClient(handler))
    //                            {
    //                                client.BaseAddress = new Uri(CMPBaseURL);
    //                                client.DefaultRequestHeaders.Add("accept", "application/json");
    //                                client.DefaultRequestHeaders.Add("authorization", "bearer " + cMPAuthTokenResponse.access_token);
    //                                var response = client.GetAsync("/api/prices?max=10000");
    //                                //var response = await client.GetAsync("/api/prices/" + plans.servicePlans[j].  [z].id);
    //                                if (response.Result.IsSuccessStatusCode)
    //                                {
    //                                    string pricingJson = response.Result.Content.ReadAsStringAsync().Result;
    //                                    pricing = JsonSerializer.Deserialize<Pricing>(pricingJson)!;
    //                                }
    //                            }
    //                        }

    //                        //getting pricing

    //                        //preparing model 
    //                        List<SubscriptionPlan> configurationList = new List<SubscriptionPlan>();
    //                        for (int i = 0; i < clouds.zones.Count; i++)
    //                        {
    //                            for (int j = 0; j < plans.servicePlans.Count; j++)
    //                            {
    //                                if (plans.servicePlans[j].provisionType != null)
    //                                {
    //                                    if (plans.servicePlans[j].zones.Count > 0)
    //                                    {
    //                                        if (clouds.zones[i].id == plans.servicePlans[j].zones[0].id)
    //                                        {
    //                                            decimal amount = 0;
    //                                            string billing = "";
    //                                            for (int z = 0; z < pricing.prices.Count; z++)
    //                                            {
    //                                                for (int x = 0; x < plans.servicePlans[j].priceSets.Count; x++)
    //                                                {
    //                                                    if (plans.servicePlans[j].priceSets.Count > 0)
    //                                                    {
    //                                                        if (plans.servicePlans[j].priceSets[x].priceUnit != "" || plans.servicePlans[j].priceSets[x].priceUnit != null)
    //                                                        {
    //                                                            //check for plan price set ID to pricing ID 
    //                                                            if (plans.servicePlans[j].priceSets[x].id == pricing.prices[z].id)
    //                                                            {
    //                                                                amount = amount + Convert.ToDecimal(pricing.prices[z].price);
    //                                                                billing = plans.servicePlans[j].priceSets[x].priceUnit;
    //                                                            }
    //                                                        }
    //                                                    }
    //                                                }
    //                                            }
    //                                            SubscriptionPlan configuration = new SubscriptionPlan();
    //                                            configuration.CloudName = clouds.zones[i].name;
    //                                            configuration.PlanName = plans.servicePlans[j].name;
    //                                            configuration.Price = Convert.ToDouble(amount);
    //                                            configurationList.Add(configuration);
    //                                        }
    //                                    }
    //                                }
    //                            }
    //                        }

    //                        foreach (var item in configurationList)
    //                        {
    //                            objects.Add(item);
    //                        }
    //                        //preparing model 
    //                    }
    //                    else
    //                    {
    //                        string apiResponse = res.Result.ToString();
    //                        cMPAuthTokenErrorResponse = JsonSerializer.Deserialize<CMPAuthTokenErrorResponse>(apiResponse)!;
    //                    }
    //                }
    //            }
    //        }
    //        e.Objects = objects;
    //    }
    //    if (e.ObjectType == typeof(UserSubscribedPlan))
    //    {
    //        // Initialize the list to be returned
    //        BindingList<UserSubscribedPlan> objects = new BindingList<UserSubscribedPlan>();
    //        CMPAuthTokenResponse cMPAuthTokenResponse = new CMPAuthTokenResponse();
    //        CMPAuthTokenErrorResponse cMPAuthTokenErrorResponse = new CMPAuthTokenErrorResponse();
    //        CMPInvoices cMPInvoices = new CMPInvoices();
    //        TenantAccounts tenantAccount = new TenantAccounts();
    //        // Replace with your actual API endpoint

    //        using (var cmpAuthTokenHandler = new HttpClientHandler())
    //        {
    //            cmpAuthTokenHandler.ServerCertificateCustomValidationCallback = (request, cert, chain, errors) => { return true; };
    //            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

    //            using (var cmpAuthTokenClient = new HttpClient(cmpAuthTokenHandler))
    //            {
    //                cmpAuthTokenClient.BaseAddress = new Uri(CMPBaseURL);
    //                var values = new Dictionary<string, string>
    //                     {
    //                         { "username", CMPUsername },
    //                         { "password", CMPPassword}
    //                     };
    //                var content = new FormUrlEncodedContent(values);

    //                using (var res = cmpAuthTokenClient.PostAsync("oauth/token?client_id=morph-api&grant_type=password&scope=write", content))
    //                {
    //                    if (res.Result.IsSuccessStatusCode)
    //                    {
    //                        string apiResponse = res.Result.Content.ReadAsStringAsync().Result;
    //                        cMPAuthTokenResponse = JsonSerializer.Deserialize<CMPAuthTokenResponse>(apiResponse)!;

    //                        //getting invoice 
    //                        using (var handler = new HttpClientHandler())
    //                        {
    //                            handler.ServerCertificateCustomValidationCallback = (request, cert, chain, errors) => { return true; };
    //                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
    //                            using (var client = new HttpClient(handler))
    //                            {
    //                                client.BaseAddress = new Uri(CMPBaseURL);
    //                                client.DefaultRequestHeaders.Add("accept", "application/json");
    //                                client.DefaultRequestHeaders.Add("authorization", "bearer " + cMPAuthTokenResponse.access_token);
    //                                DateTime now = DateTime.Now;
    //                                DateTime currentMonthStart = new DateTime(now.Year, now.Month, 1);
    //                                DateTime currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);
    //                                string startDate = currentMonthStart.ToString("yyyy-MM-dd");
    //                                string endDate = currentMonthEnd.ToString("yyyy-MM-dd");
    //                                using (var invoicesResponse = client.GetAsync("api/invoices?max=1000000000000000000&offset=0&sort=refName&direction=asc&startDate=" + startDate + "&endDate=" + endDate + "&includeLineItems=false&includeTotals=true"))
    //                                {
    //                                    if (invoicesResponse.Result.IsSuccessStatusCode)
    //                                    {
    //                                        string invoicesResponseJson = invoicesResponse.Result.Content.ReadAsStringAsync().Result;
    //                                        cMPInvoices = JsonSerializer.Deserialize<CMPInvoices>(invoicesResponseJson)!;
    //                                        //var filteredByName = invoices.invoices.Where(p => p.account.id == 1).ToList();
    //                                        Console.WriteLine(apiResponse);
    //                                    }
    //                                    else
    //                                    {
    //                                        string invoicesResponseJson = invoicesResponse.Result.Content.ReadAsStringAsync().Result;
    //                                        Console.WriteLine(apiResponse);
    //                                    }
    //                                }
    //                            }
    //                        }
    //                        //getting invoice 

    //                        //getting Tenant
    //                        using (var handler1 = new HttpClientHandler())
    //                        {
    //                            handler1.ServerCertificateCustomValidationCallback = (request, cert, chain, errors) => { return true; };
    //                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
    //                            using (var client1 = new HttpClient(handler1))
    //                            {
    //                                client1.BaseAddress = new Uri(CMPBaseURL);
    //                                client1.DefaultRequestHeaders.Add("accept", "application/json");
    //                                client1.DefaultRequestHeaders.Add("authorization", "bearer " + cMPAuthTokenResponse.access_token);
    //                                using (var tenantReponse = client1.GetAsync("api/accounts?max=1000000000&offset=0&sort=name&direction=asc"))
    //                                {
    //                                    if (tenantReponse.Result.IsSuccessStatusCode)
    //                                    {
    //                                        string tenantResponseJson = tenantReponse.Result.Content.ReadAsStringAsync().Result;
    //                                        tenantAccount = JsonSerializer.Deserialize<TenantAccounts>(tenantResponseJson)!;
    //                                        Console.WriteLine(apiResponse);
    //                                    }
    //                                    else
    //                                    {
    //                                        string tenantResponseJson = tenantReponse.Result.Content.ReadAsStringAsync().Result;
    //                                        Console.WriteLine(apiResponse);
    //                                    }
    //                                }
    //                            }
    //                        }
    //                        List<UserSubscribedPlan> configurationList = new List<UserSubscribedPlan>();
    //                        for (int i = 0; i < tenantAccount.accounts.Count; i++)
    //                        {
    //                            if (tenantAccount.accounts[i].id != 1)
    //                            {

    //                                for (int j = 0; j < cMPInvoices.invoices.Count; j++)
    //                                {
    //                                    if (tenantAccount.accounts[i].id == cMPInvoices.invoices[j].account.id)
    //                                    {
    //                                        UserSubscribedPlan configuration = new UserSubscribedPlan();
    //                                        configuration.TenantName = tenantAccount.accounts[i].name;
    //                                        configuration.PlanName = cMPInvoices.invoices[j].plan == null ? "" : cMPInvoices.invoices[j].plan.name;

    //                                        configurationList.Add(configuration);
    //                                    }
    //                                }
    //                            }
    //                        }
    //                        foreach (var item in configurationList)
    //                        {
    //                            objects.Add(item);
    //                        }
    //                        //preparing model 
    //                    }
    //                    else
    //                    {
    //                        string apiResponse = res.Result.ToString();
    //                        cMPAuthTokenErrorResponse = JsonSerializer.Deserialize<CMPAuthTokenErrorResponse>(apiResponse)!;
    //                    }
    //                }
    //            }
    //        }

    //        e.Objects = objects;
    //    }

    //    if (e.ObjectType == typeof(Invoice))
    //    {
    //        BindingList<Invoice> objects = new BindingList<Invoice>();
    //        List<CRMInvoice> cRMInvoices = new List<CRMInvoice>();
    //        try
    //        {
    //            var handler = new HttpClientHandler
    //            {
    //                ServerCertificateCustomValidationCallback = (request, cert, chain, errors) => true
    //            };

    //            using (var client = new HttpClient(handler))
    //            {
    //                client.BaseAddress = new Uri(BillingMIddleWareAPIs);
    //                client.DefaultRequestHeaders.Add("accept", "application/json");

    //                var response = client.GetAsync($"InvoicesAndSubscriptions/getInvoicesForCRM");

    //                if (response.Result.IsSuccessStatusCode)
    //                {
    //                    string cloudJson = response.Result.Content.ReadAsStringAsync().Result;
    //                    cRMInvoices = JsonSerializer.Deserialize<List<CRMInvoice>>(cloudJson)!;
    //                    List<Invoice> configurationList = new List<Invoice>();
    //                    if (cRMInvoices.Count > 0)
    //                    {
    //                        for (int i = 0; i < cRMInvoices.Count; i++)
    //                        {

    //                            Invoice configuration = new Invoice();
    //                            configuration.InvoiceNumber = cRMInvoices[i].invoiceNumber;
    //                            configuration.TenantName = cRMInvoices[i].tenantName;
    //                            configuration.InvoiceDate = cRMInvoices[i].invoiceDate;
    //                            configuration.Amount = Convert.ToDouble(cRMInvoices[i].amount);
    //                            configuration.Currency = "$";
    //                            configuration.Status = cRMInvoices[i].status;

    //                            configurationList.Add(configuration);


    //                        }

    //                        foreach (var item in configurationList)
    //                        {
    //                            objects.Add(item);
    //                        }
    //                    }
    //                    else
    //                    {
    //                        objects.Add(new Invoice
    //                        {
    //                            InvoiceNumber = "No Record",
    //                        });
    //                    }
    //                }
    //                else
    //                {
    //                    objects.Add(new Invoice
    //                    {
    //                        InvoiceNumber = "ERROR",
    //                        TenantName = "Call Failed",
    //                        Status = response.Result.StatusCode.ToString(),
    //                    });
    //                }
    //            }

    //        }
    //        catch (Exception ex)
    //        {
    //            // Exception – show exception message
    //            objects.Add(new Invoice
    //            {
    //                InvoiceNumber = "EXCEPTION",
    //                TenantName = "Error occurred",
    //                Status = ex.Message,
    //            });
    //        }
    //        e.Objects = objects;
    //    }
    //}

    private void NonPersistentObjectSpace_ObjectsGetting(object sender, ObjectsGettingEventArgs e)
    {
        string CMPBaseURL = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("URls")["CMP"]!;
        string BillingMIddleWareAPIs = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("URls")["BillingMIddleWareAPIs"]!;
        string CMPUsername = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Credentials")["CMPUsername"]!;
        string CMPPassword = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Credentials")["CMPPassword"]!;

        using var httpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (request, cert, chain, errors) => true
        });

        if (e.ObjectType == typeof(SubscriptionPlan))
        {
            BindingList<SubscriptionPlan> objects = [];
            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (request, cert, chain, errors) => true
                };

                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri(BillingMIddleWareAPIs);

                    var response = client.GetAsync("cmp/subscription-plans");

                    if (response.Result.IsSuccessStatusCode)
                    {
                        string cloudJson = response.Result.Content.ReadAsStringAsync().Result;
                        var rawResponse = JsonSerializer.Deserialize<SubscriptionResponse>(cloudJson)!;
                        if (rawResponse.data is null || rawResponse.data.Count == 0)
                        {
                            objects.Add(new SubscriptionPlan
                            {
                                CloudName = "No Record",
                                PlanName = "No Record",
                                Price = "No Record"
                            });
                        }
                        else
                        {
                            foreach (var item in rawResponse.data)
                            {
                                objects.Add(new SubscriptionPlan
                                {
                                    CloudName = item.provisionType,
                                    PlanName = item.planName,
                                    Price = item.price
                                });
                            }
                        }
                    }
                    else
                    {
                        objects.Add(new SubscriptionPlan
                        {
                            CloudName = "ERROR",
                            PlanName = "Call Failed",
                            Price = response.Result.StatusCode.ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {

                objects.Add(new SubscriptionPlan
                {
                    CloudName = "EXCEPTION",
                    PlanName = "Error occurred",
                    Price = ex.Message
                });
            }
            e.Objects = objects;
        }

        if (e.ObjectType == typeof(UserSubscribedPlan))
        {
            BindingList<UserSubscribedPlan> objects = [];

            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (request, cert, chain, errors) => true
                };

                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri(BillingMIddleWareAPIs);

                    var response = client.GetAsync("cmp/user-subscriptions");

                    if (response.Result.IsSuccessStatusCode)
                    {
                        string rawJson = response.Result.Content.ReadAsStringAsync().Result;
                        var userPlanList = JsonSerializer.Deserialize<UserSubscriptionsResponse>(rawJson)!;
                        if (userPlanList.data is not null && userPlanList.data.Count > 0)
                        {
                            foreach (var plan in userPlanList.data)
                            {
                                foreach (var tenant in plan.tenants)
                                {
                                    objects.Add(new UserSubscribedPlan
                                    {
                                        PlanName = plan.planName,
                                        TenantName = tenant
                                    });
                                }
                            }
                        }
                        else
                        {
                            objects.Add(new UserSubscribedPlan
                            {
                                PlanName = "No Record",
                                TenantName = "No Record"
                            });
                        }
                    }
                    else
                    {
                        objects.Add(new UserSubscribedPlan
                        {
                            PlanName = "EXCEPTION",
                            TenantName = response.Result.StatusCode.ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                objects.Add(new UserSubscribedPlan
                {
                    PlanName = "EXCEPTION",
                    TenantName = ex.Message
                });
            }
            e.Objects = objects;
        }

        // Invoice portion
        if (e.ObjectType == typeof(Invoice))
        {
            BindingList<Invoice> objects = new BindingList<Invoice>();
            List<CRMInvoice> cRMInvoices = new List<CRMInvoice>();
            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (request, cert, chain, errors) => true
                };

                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri(BillingMIddleWareAPIs);

                    var response = client.GetAsync("invoices/crm");

                    if (response.Result.IsSuccessStatusCode)
                    {
                        string cloudJson = response.Result.Content.ReadAsStringAsync().Result;
                        var responseModal = JsonSerializer.Deserialize<InvoicesResponse>(cloudJson)!;
                        cRMInvoices = responseModal.data;
                        if (cRMInvoices.Count > 0)
                        {
                            foreach (var inv in cRMInvoices)
                            {
                                objects.Add(new Invoice
                                {
                                    InvoiceNumber = inv.invoiceNumber,
                                    TenantName = inv.tenantName,
                                    //InvoiceDate = inv.invoiceDate,
                                    Amount = Convert.ToDouble(inv.amount),
                                    Currency = "USD",
                                    Status = inv.status
                                });
                            }
                        }
                        else
                        {
                            objects.Add(new Invoice { InvoiceNumber = "No Record" });
                        }
                    }
                    else
                    {
                        objects.Add(new Invoice
                        {
                            InvoiceNumber = "ERROR",
                            TenantName = "Call Failed",
                            Status = response.Result.StatusCode.ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                objects.Add(new Invoice
                {
                    InvoiceNumber = "EXCEPTION",
                    TenantName = "Error occurred",
                    Status = ex.Message
                });
            }
            e.Objects = objects;
        }
    }
}