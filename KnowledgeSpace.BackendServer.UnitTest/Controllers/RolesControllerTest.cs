using KnowledgeSpace.BackEndServer.Controllers;
using KnowledgeSpace.ViewModels;
using KnowledgeSpace.ViewModels.Systems;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MockQueryable.Moq;
using MockQueryable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeSpace.BackendServer.UnitTest.Controllers
{
	public class RolesControllerTest
	{
		private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
		private List<IdentityRole> _roleSources = new List<IdentityRole>()
				{
					new IdentityRole("Test 1"),
					new IdentityRole("Test 2"),
					new IdentityRole("Test 3"),
					new IdentityRole("Test 4")
				};

		public RolesControllerTest()
		{
			var roleStore = new Mock<IRoleStore<IdentityRole>>();
			_mockRoleManager = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null, null, null, null);
		}

		// Test case to verify that the RolesController can be instantiated successfully
		[Fact]
		public void ShouldCreateInstance_NotNull_Success()
		{
			var roleController = new RolesController(_mockRoleManager.Object);
			Assert.NotNull(roleController);
		}

		// Test case to verify that the PostRole method returns a CreatedAtActionResult when provided with valid input
		[Fact]
		public async Task PostRole_ValidInput_Success()
		{
			_mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);
			var roleController = new RolesController(_mockRoleManager.Object);
			var result = await roleController.PostRole(new RoleViewModel() { Id = "test", Name = "test" });
			Assert.NotNull(result);
			Assert.IsType<CreatedAtActionResult>(result);
		}

		[Fact]
		public async Task PostRole_ValidInput_Failed()
		{
			_mockRoleManager.Setup(
				x => x.CreateAsync(It.IsAny<IdentityRole>()))
				.ReturnsAsync(IdentityResult.Failed(new IdentityError[] { }));
			var rolesController = new RolesController(_mockRoleManager.Object);
			var result = await rolesController.PostRole(new RoleViewModel()
			{
				Id = "test",
				Name = "test"
			});

			Assert.NotNull(result);
			Assert.IsType<BadRequestObjectResult>(result);
		}

		[Fact]
		public async Task GetRoles_HasData_ReturnSuccess()
		{
			_mockRoleManager.Setup(x => x.Roles).Returns(_roleSources.AsQueryable().BuildMock()); //
			var rolesController = new RolesController(_mockRoleManager.Object);
			var result = await rolesController.GetRoles();
			var okResult = result as OkObjectResult;
			var roleViewModels = okResult.Value as IEnumerable<RoleViewModel>;
			Assert.True(roleViewModels.Count() > 0);
		}

		[Fact]
		public async Task GetRoles_ThrowException_Failed()
		{
			_mockRoleManager.Setup(x => x.Roles).Throws<Exception>();
			var rolesController = new RolesController(_mockRoleManager.Object);
			await Assert.ThrowsAnyAsync<Exception>(async () => await rolesController.GetRoles());
		}

		[Fact]
		public async Task GetRolesPaging_NoFilter_ReturnSuccess()
		{
			_mockRoleManager.Setup(x => x.Roles).Returns(_roleSources.AsQueryable().BuildMock()); //
			
			var rolesController = new RolesController(_mockRoleManager.Object);
			var result = await rolesController.GetRolesPaging(null, 1, 2);
			var okResult = result as OkObjectResult;
			var roleViewModels = okResult.Value as Pagination<RoleViewModel  >;
			Assert.Equal(4, roleViewModels.TotalRecords);
			Assert.Equal(2, roleViewModels.Items.Count);
		}

		//[Fact]
		//public async Task GetRolesPaging_HasFilter_ReturnSuccess()
		//{
		//	_mockRoleManager.Setup(x => x.Roles).Returns(_roleSources.AsQueryable().BuildMock()); //Object

		//	var rolesController = new RolesController(_mockRoleManager.Object);
		//	var result = await rolesController.GetRolesPaging("Test3", 1, 2);
		//	var okResult = result as OkObjectResult;
		//	var roleViewModels = okResult.Value as Pagination<RoleViewModel>;

		//	if (roleViewModels == null)
		//	{
		//		Assert.False(false);
		//	}

		//	Assert.Equal(1, roleViewModels.TotalRecords); //error
		//	Assert.Single(roleViewModels.Items);
		//}

		[Fact]
		public async Task GetRolesPaging_ThrowException_Failed()
		{
			_mockRoleManager.Setup(x => x.Roles).Throws<Exception>();

			var rolesController = new RolesController(_mockRoleManager.Object);

			await Assert.ThrowsAnyAsync<Exception>(async () => await rolesController.GetRolesPaging(null, 1, 2));
		}

		[Fact]
		public async Task GetById_HasData_ReturnSuccess()
		{
			_mockRoleManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
				.ReturnsAsync(new IdentityRole()
				{
					Id = "test1",
					Name = "test1"
				});

			var rolesController = new RolesController(_mockRoleManager.Object);
			var result = await rolesController.GetById("test1");
			var okResult = result as OkObjectResult;
			Assert.NotNull(okResult);

			var roleViewModels = okResult.Value as RoleViewModel;
			Assert.Equal("test1", roleViewModels.Name);
		}

		[Fact]
		public async Task GetById_ThrowException_Failed()
		{
			_mockRoleManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).Throws<Exception>();
			var rolesController = new RolesController(_mockRoleManager.Object);
			await Assert.ThrowsAnyAsync<Exception>(async () => await rolesController.GetById("test1"));
		}

		[Fact]
		public async Task PutRole_ValidInput_Success()
		{
			_mockRoleManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
				.ReturnsAsync(new IdentityRole()
				{
					Id = "test",
					Name = "test"
				});

			_mockRoleManager.Setup(x => x.UpdateAsync(It.IsAny<IdentityRole>()))
				.ReturnsAsync(IdentityResult.Success);

			var roleController = new RolesController(_mockRoleManager.Object);
			var result = await roleController.
				PutRole("test", new RoleViewModel() { Id = "test", Name = "test" });

			Assert.NotNull(result);
			Assert.IsType<NoContentResult>(result);
		}

		[Fact]
		public async Task PutRole_ValidInput_Failed()
		{
			_mockRoleManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
				.ReturnsAsync(new IdentityRole()
				{
					Id = "test",
					Name = "test"
				});

			_mockRoleManager.Setup(x => x.UpdateAsync(It.IsAny<IdentityRole>()))
				.ReturnsAsync(IdentityResult.Failed(new IdentityError[] { }));

			var rolesController = new RolesController(_mockRoleManager.Object);
			var result = await rolesController
				.PutRole("test", new RoleViewModel() { Id = "test", Name = "test" });

			Assert.NotNull(result);
			Assert.IsType<BadRequestObjectResult>(result);
		}

		[Fact]
		public async Task DeleteRole_ValidInput_Success()
		{
			_mockRoleManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
				.ReturnsAsync(new IdentityRole()
				{
					Id = "test",
					Name = "test"
				});

			_mockRoleManager.Setup(x => x.DeleteAsync(It.IsAny<IdentityRole>()))
				.ReturnsAsync(IdentityResult.Success);

			var roleController = new RolesController(_mockRoleManager.Object);
			var result = await roleController.
				DeleteRole("test");

			Assert.IsType<OkObjectResult>(result);
		}

		[Fact]
		public async Task DeleteRole_ValidInput_Failed()
		{
			_mockRoleManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
				.ReturnsAsync(new IdentityRole()
				{
					Id = "test",
					Name = "test"
				});

			_mockRoleManager.Setup(x => x.DeleteAsync(It.IsAny<IdentityRole>()))
				.ReturnsAsync(IdentityResult.Failed(new IdentityError[] { }));

			var rolesController = new RolesController(_mockRoleManager.Object);
			var result = await rolesController
				.DeleteRole("test");

			Assert.IsType<BadRequestObjectResult>(result);
		}
	}
}
