using KnowledgeSpace.ViewModels.Systems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeSpace.BackEndServer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	//[Authorize("Bearer")]
	public class UsersController1 : ControllerBase
	{
		private readonly UserManager<IdentityUser> _userManager;

		public UsersController1(UserManager<IdentityUser> userManager)
		{
			_userManager = userManager;
		}

		[HttpPost]
		public async Task<IActionResult> PostUser(UserViewModel userViewModel)
		{
			var user = new IdentityUser()
			{
				FirstName = userViewModel.FirstName,
				LastName = userViewModel.LastName,
				Dob = userViewModel.Dob,
				NumberOfKnowledgeBases = userViewModel.NumberOfKnowledgeBases,
				NumberOfVotes = userViewModel.NumberOfVotes,
				NumberOfReports = userViewModel.NumberOfReports
			};

			var result = await _userManager.CreateAsync(user);

			if(result.Succeeded){
				return CreatedAtAction(nameof(GetById), new { id = user.Id }, userViewModel);
			}

			else
			{
				return BadRequest(result.Errors);
			}
		}

		[HttpGet]
		public async Task<IActionResult> GetUsers()
		{
			var users = _userManager.Users;

			var userViewModels = await users.Select(u => new UserViewModel()
			{
				FirstName = u.FirstName,
				LastName = u.LastName,
				Dob = u.Dob,
				NumberOfKnowledgeBases = u.NumberOfKnowledgeBases,
				NumberOfVotes = u.NumberOfVotes,
				NumberOfReports = u.NumberOfReports
			}).ToListAsync();

			return Ok(userViewModels);
		}
	}
}
