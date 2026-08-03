using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KnowledgeSpace.BackEndServer.Data
{
	public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
	{
		public ApplicationDbContext CreateDbContext(string[] args)
		{
			//1. Lấy giá trị biến môi trường, xác định môi trường hiện tại (Development, Staging, Production)
			var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

			//2. Xây dựng cấu hình
			// - ConfigurationBuilder là công cụ chuyên dụng để đọc cấu hình
			IConfigurationRoot configuration = new ConfigurationBuilder()
				//Chỉ định thư mục gốc của dự án: Thư mục hiện tại
				.SetBasePath(Directory.GetCurrentDirectory())
				//Đọc file cấu hình appsettings.json
				.AddJsonFile("appsettings.json")
				//Đọc file cấu hình appsettings.{environmentName}.json, ví dụ: appsettings.Development.json
				.AddJsonFile($"appsettings.{environmentName}.json")
				//Biên dịch các biến môi trường
				.Build();

			//3. Cấu hình DBContext và trả kết quả
			//Biến builder chứa và quản lý các thiết lập (Options) cho ApplicationDbContext
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			//Sử dụng đối tượng configuration đã nạp data để tìm và trích xuất
			//Connection String có tên là "DefaultConnection"
			var connectionString = configuration.GetConnectionString("DefaultConnection");
			builder.UseSqlServer(connectionString);
			//4. Trả về đối tượng ApplicationDbContext với các thiết lập đã được cấu hình cho EF core sử dụng
			return new ApplicationDbContext(builder.Options);
		}
	}
}
