using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeSpace.ViewModels.Systems
{
	public class RoleViewModelValidator : AbstractValidator<RoleViewModel>
	{
		public RoleViewModelValidator()
		{
			RuleFor(x => x.Id).NotEmpty().WithMessage("Id value is required.")
				.MaximumLength(50).WithMessage("Role Id cannot over limit 50 characters.");
			RuleFor(x => x.Name).NotEmpty().WithMessage("Name value is required.");
		}
	}
}
