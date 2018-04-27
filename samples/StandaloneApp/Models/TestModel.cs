using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StandaloneApp.Models
{
	public class TestModel
	{
		[Required]
		[MaxLength(255)]
		[Display(Name = "Display Text")]
		public string Text { get; set; }

		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; } = "pippo";
	}
}
