using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeSpace.ViewModels.Systems
{
	public class UserViewModel
	{
		public string FirstName { get; set; }

		public string LastName { get; set; }

		public DateTime Dob { get; set; }

		public int? NumberOfKnowledgeBases { get; set; }

		public int? NumberOfVotes { get; set; }

		public int? NumberOfReports { get; set; }
	}
}
