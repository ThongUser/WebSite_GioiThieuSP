using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebSite_GioiThieuSP.Models
{
	[Index(nameof(Username), IsUnique = true)]
	public class Users
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(50)]
		public string Username { get; set; } = string.Empty;

		[Required]
		[StringLength(255)]
		public string Password { get; set; } = string.Empty;

		[Required]
		[StringLength(100)]
		public string FullName { get; set; } = string.Empty;

		[EmailAddress]
		[StringLength(100)]
		[Column(TypeName = "varchar(100)")]
		public string? Email { get; set; }

		[Required]
		[StringLength(20)]
		public string Role { get; set; } = "Customer";

		[Required]
		public bool IsActive { get; set; }
	}
}
