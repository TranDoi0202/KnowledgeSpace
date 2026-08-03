using FluentValidation;
using FluentValidation.AspNetCore;
using KnowledgeSpace.BackendServer.Data.Entities;
using KnowledgeSpace.BackEndServer.Data;
using KnowledgeSpace.BackEndServer.IdentityServer;
using KnowledgeSpace.BackEndServer.Services;
using KnowledgeSpace.ViewModels.Systems;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
// Cần using thêm namespace chứa file Config (nơi định nghĩa Apis, Clients, Ids)
// using KnowledgeSpace.BackendServer.IdentityServer; 

// 1. Cấu hình Serilog ở ngay đầu file
Log.Logger = new LoggerConfiguration()
	.Enrich.FromLogContext()
	.WriteTo.Console()
	.CreateLogger();

try
{
	var builder = WebApplication.CreateBuilder(args);

	// Tích hợp Serilog vào hệ thống của .NET
	builder.Host.UseSerilog();

	// Add services to the container.
	builder.Services.AddControllers();

	// Kích hoạt tính năng tự động Validate
	builder.Services.AddFluentValidationAutoValidation();

	// Quét và đăng ký tất cả các class kế thừa từ AbstractValidator
	builder.Services.AddValidatorsFromAssemblyContaining<RoleViewModelValidator>();

	builder.Services.AddEndpointsApiExplorer();

	// ========================================================
	// CẤU HÌNH DATABASE VÀ IDENTITY
	// ========================================================

	// 1. Setup Entity Framework
	builder.Services.AddDbContext<ApplicationDbContext>(options =>
		options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

	// 2. Setup Identity
	builder.Services.AddIdentity<User, IdentityRole>()
		.AddEntityFrameworkStores<ApplicationDbContext>()
		.AddDefaultTokenProviders();

	// 3. Configure Identity Options
	builder.Services.Configure<IdentityOptions>(options =>
	{
		options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
		options.Lockout.MaxFailedAccessAttempts = 5;
		options.Lockout.AllowedForNewUsers = true;
		options.SignIn.RequireConfirmedPhoneNumber = false;
		options.SignIn.RequireConfirmedAccount = false;
		options.SignIn.RequireConfirmedEmail = false;
		options.Password.RequiredLength = 8;
		options.Password.RequireDigit = true;
		options.Password.RequireUppercase = true;
		options.User.RequireUniqueEmail = true;
	});

	// ========================================================
	// CẤU HÌNH IDENTITY SERVER (Chuyển từ .NET 3.0)
	// ========================================================
	builder.Services.AddIdentityServer(options =>
	{
		options.Events.RaiseErrorEvents = true;
		options.Events.RaiseInformationEvents = true;
		options.Events.RaiseFailureEvents = true;
		options.Events.RaiseSuccessEvents = true;

	})
	.AddInMemoryApiScopes(Config.ApiScopes)
	.AddInMemoryApiResources(Config.Apis)
	.AddInMemoryClients(Config.Clients)
	.AddInMemoryIdentityResources(Config.Ids)
	.AddAspNetIdentity<User>()
	.AddDeveloperSigningCredential();
	// Lưu ý: Nếu khóa học dùng phiên bản IdentityServer4 có hàm sinh khóa ảo, 
	// bạn có thể cần thêm .AddDeveloperSigningCredential() ở cuối chuỗi cấu hình trên.

	// ========================================================

	builder.Services.AddTransient<DbInitializer>();
	// Nhớ bổ sung thêm EmailSenderService nếu bạn đã có class này
	// builder.Services.AddTransient<IEmailSender, EmailSenderService>();

	builder.Services.AddSwaggerGen(c =>
	{
		c.SwaggerDoc("v1", new OpenApiInfo { Title = "Knowledge Space API", Version = "v1" });

		c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
		{
			Type = SecuritySchemeType.OAuth2,
			Flows = new OpenApiOAuthFlows
			{
				Implicit = new OpenApiOAuthFlow
				{
					AuthorizationUrl = new Uri("https://localhost:7156/connect/authorize"),
					Scopes = new Dictionary<string, string> { { "api.knowledgespace", "KnowledgeSpace API" } }
				},
			},
		});

		c.AddSecurityRequirement(new OpenApiSecurityRequirement
		{
			{
				new OpenApiSecurityScheme
				{
					Reference = new OpenApiReference
					{
						Type = ReferenceType.SecurityScheme,
						Id = "Bearer"
					}
				},
				new List<string>{ "api.knowledgespace" }
			}
		});
	});

	//builder.Services.AddRazorPages(options =>
	//{
	//	options.Conventions.AddAreaFolderRouteModelConvention("Identity", "/Account/", model =>
	//	{
	//		foreach (var selector in model.Selectors)
	//		{
	//			var attributeRouteModel = selector.AttributeRouteModel;
	//			attributeRouteModel.Order = -1;
	//			attributeRouteModel.Template = attributeRouteModel.Template.Remove(0, "Identity".Length);
	//		}
	//	});
	//});

	// 1. Cấu hình lại Cookie để báo cho hệ thống biết đường dẫn mới
	builder.Services.ConfigureApplicationCookie(options =>
	{
		options.LoginPath = "/Account/Login";
		options.LogoutPath = "/Account/Logout";
		options.AccessDeniedPath = "/Account/AccessDenied";
	});

	// 2. Cập nhật lại đoạn code định tuyến của bạn (thêm bước kiểm tra cho an toàn)

	builder.Services.AddAuthentication().AddLocalApi("Bearer", options =>
	{
		options.ExpectedScope = "api.knowledgespace";
	});

	builder.Services.AddAuthorization(option =>
	{
		option.AddPolicy("Bearer", policy =>
		{
			policy.AddAuthenticationSchemes("Bearer");
			policy.RequireAuthenticatedUser();
		});
	});

	builder.Services.AddRazorPages(options =>
	{
		options.Conventions.AddAreaFolderRouteModelConvention("Identity", "/Account/", model =>
		{
			foreach (var selector in model.Selectors)
			{
				var attributeRouteModel = selector.AttributeRouteModel;
				// Đảm bảo Template không bị null và chắc chắn bắt đầu bằng chữ "Identity" trước khi cắt
				if (attributeRouteModel.Template != null && attributeRouteModel.Template.StartsWith("Identity"))
				{
					attributeRouteModel.Template = attributeRouteModel.Template.Substring("Identity".Length);
				}
			}
		});
	});

	builder.Services.AddTransient<IEmailSender, EmailSenderService>();

	var app = builder.Build();

	// 2. Thực hiện quá trình Seeding Data
	using (var scope = app.Services.CreateScope())
	{
		var services = scope.ServiceProvider;
		try
		{
			Log.Information("Seeding data...");
			var dbInitializer = services.GetRequiredService<DbInitializer>();
			await dbInitializer.Seed();
		}
		catch (Exception ex)
		{
			var logger = services.GetRequiredService<ILogger<Program>>();
			logger.LogError(ex, "An error occurred while seeding the database.");
		}
	}

	// Configure the HTTP request pipeline.
	if (app.Environment.IsDevelopment())
	{
		app.UseSwagger();
		app.UseSwaggerUI(c =>
		{
			c.OAuthClientId("swagger");
			c.SwaggerEndpoint("/swagger/v1/swagger.json", "Knowledge Space API V1");
		});
	}

	app.UseHttpsRedirection();
	app.UseStaticFiles();

	app.UseRouting();

	// ⚠️ QUAN TRỌNG: Kích hoạt IdentityServer trong Pipeline
	// app.UseIdentityServer() bao hàm cả chức năng xác thực, nên nó sẽ thay thế app.UseAuthentication()
	app.UseIdentityServer();
	app.UseAuthorization();

	app.MapDefaultControllerRoute();
	app.MapRazorPages();

	// 3. Chạy ứng dụng
	app.Run();
}
catch (Exception ex)
{
	Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
	Log.CloseAndFlush();
}

//builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
//builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
