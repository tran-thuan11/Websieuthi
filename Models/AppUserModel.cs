﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models
{
	public class AppUserModel : IdentityUser
	{
		public string Occuapation { get; set; }
		public string RoleId { get; set; }

		public string Token { get; set; }

	}
}
