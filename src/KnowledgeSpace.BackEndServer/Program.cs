using KnowledgeSpace.BackendServer.Data.Entities;
using KnowledgeSpace.BackEndServer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using FluentValidation;
using FluentValidation.AspNetCore;
using KnowledgeSpace.ViewModels.Systems;

// Make sure to add the using directive for wherever your 'User' class is located

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

	// 1. Kích hoạt tính năng tự động Validate (để giữ nguyên hành vi tự trả về lỗi 400 giống .NET 3.0)
	builder.Services.AddFluentValidationAutoValidation();

	// (Tùy chọn) Kích hoạt hỗ trợ validate cho phía Client nếu bạn có dùng Razor Pages / MVC Views
	// builder.Services.AddFluentValidationClientsideAdapters(); 

	// 2. Quét và đăng ký tất cả các class kế thừa từ AbstractValidator nằm cùng thư mục/project với RoleVmValidator
	builder.Services.AddValidatorsFromAssemblyContaining<RoleViewModelValidator>();

	builder.Services.AddEndpointsApiExplorer();

	// ========================================================
	// NEW CODE TRANSLATED FROM THE MENTOR'S .NET 3.0 TUTORIAL
	// ========================================================

	// 1. Setup entity framework
	// Note: Change 'ApplicationDbContext' if you named your context class something else (e.g., AppDbContext)
	builder.Services.AddDbContext<ApplicationDbContext>(options =>
		options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

	// 2. Setup identity
	// Note: Change 'User' if your user class has a different name (e.g., AppUser, ApplicationUser)
	builder.Services.AddIdentity<User, IdentityRole>()
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddDefaultTokenProviders(); // <-- Bổ sung hàm này

	// 3. Configure Identity Options
	builder.Services.Configure<IdentityOptions>(options =>
	{
		// Default Lockout settings.
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

	// ⚠️ QUAN TRỌNG: Bạn cần đăng ký DbInitializer vào Dependency Injection (DI) ở đây
	// ⚠️ QUAN TRỌNG: Bạn cần đăng ký DbInitializer vào Dependency Injection (DI) ở đây
	builder.Services.AddTransient<DbInitializer>();

	builder.Services.AddSwaggerGen(c =>
	{
		c.SwaggerDoc("v1", new OpenApiInfo { Title = "Knowledge Space API", Version = "v1" });
	});

	builder.Services.AddRazorPages();

	var app = builder.Build();

	app.UseStaticFiles();
	app.UseAuthentication();


	// 2. Thực hiện quá trình Seeding Data (Khởi tạo dữ liệu vào Database)
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
	// Configure the HTTP request pipeline.
	if (app.Environment.IsDevelopment())
	{
		app.UseSwagger();
		app.UseSwaggerUI(c =>
		{
			c.SwaggerEndpoint("/swagger/v1/swagger.json", "Knowledge Space API V1");
		});
	}

	app.UseHttpsRedirection();

	// Ensure Authentication runs BEFORE Authorization
	app.UseAuthentication();
	app.UseAuthorization();

	app.MapControllers();
	app.MapControllers();
	app.MapRazorPages(); // <-- Bổ sung dòng này để nhận diện link /Identity/...

	// 3. Chạy ứng dụng
	app.Run();
}
catch (Exception ex)
{
	// Bắt lỗi nghiêm trọng khiến ứng dụng không thể khởi động
	Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
	// Đảm bảo tất cả các log đang kẹt trong bộ nhớ được ghi hết ra Console/File trước khi tắt app
	Log.CloseAndFlush();
}
//builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
