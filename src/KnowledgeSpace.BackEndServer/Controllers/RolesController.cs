using KnowledgeSpace.ViewModels;
using KnowledgeSpace.ViewModels.Systems;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

//There are 2 option to CRUD API. use DbContext or RoleManager
namespace KnowledgeSpace.BackEndServer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RolesController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> _roleManager;

		public RolesController(RoleManager<IdentityRole> roleManager)
		{
			_roleManager = roleManager;
		}

		// Post: api/Roles
		[HttpPost]
		public async Task<IActionResult> PostRole(RoleViewModel roleViewModel)
		{
			var role = new IdentityRole()
			{
				Id = roleViewModel.Id,
				Name = roleViewModel.Name,
				NormalizedName = roleViewModel.Name.ToUpper()
			};

			var result = await _roleManager.CreateAsync(role);

			if(result.Succeeded){
				return CreatedAtAction(nameof(GetById), new {id = role.Id }, roleViewModel);
			}
			else
			{
				return BadRequest(result.Errors);
			}
		}

		//GET: api/roles
		[HttpGet]
		public async Task<IActionResult> GetRoles()
		{
			var roles = _roleManager.Roles;

			var roleViewModels = await roles.Select(r => new RoleViewModel() {
				Id = r.Id, 
				Name = r.Name 
			}).ToListAsync();

			return Ok(roleViewModels);
		}

		//GET: api/Roles/?filter={filter}&pageIndex=1&pageSize=10
		[HttpGet("filter")]
		public async Task<IActionResult> GetRolesPaging(string filter, int pageIndex, int pageSize)
		{
			var query = _roleManager.Roles;

			if(query == null)
			{
				return NotFound();
			}

			if (!string.IsNullOrEmpty(filter))
			{
				query = query.Where(x => x.Id.Contains(filter) || x.Name.Contains(filter));
			}

			var totalRecords = await query.CountAsync();

			var items = await query.Skip(pageIndex - 1 * pageSize).Take(pageSize).Select(r => new RoleViewModel() { Id = r.Id, Name = r.Name }).ToListAsync();

			var pagination = new Pagination<RoleViewModel>
			{
				Items = items,
				TotalRecords = totalRecords
			};

			return Ok(pagination);
		}

		//GET: api/Roles/{id}
		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(string id)
		{
			var role = await _roleManager.FindByIdAsync(id);

			if(role == null)
			{
				return NotFound();
			}

			var roleViewModel = new RoleViewModel()
			{
				Id = role.Id,
				Name = role.Name
			};

			return Ok(roleViewModel);
		}

		//PUT: api/Roles/{id}
		[HttpPut("{id}")]
		public async Task<IActionResult> PutRole(string id, [FromBody] RoleViewModel roleViewModel)
		{
			if (id != roleViewModel.Id)
			{
				return BadRequest();
			}
			
			var role = await _roleManager.FindByIdAsync(id);

			if(role == null)
			{
				return NotFound();
			}

			role.Name = roleViewModel.Name;
			role.NormalizedName = roleViewModel.Name?.ToUpper();

			var result = await _roleManager.UpdateAsync(role);

			if(result.Succeeded)
			{
				return NoContent();
			}
		
			return BadRequest(result.Errors);
		}

		//DELETE: api/Roles/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteRole(string id)
		{
			var role = await _roleManager.FindByIdAsync(id);

			if(role == null)
			{
				return NotFound();
			}

			var result = await _roleManager.DeleteAsync(role);

			if (result.Succeeded)
			{
				var roleVM = new RoleViewModel()
				{
					Id = role.Id,
					Name = role.Name
				};

				return Ok(roleVM);
			}
			return BadRequest(result.Errors);
		}
	}
}
