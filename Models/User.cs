using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.Models;

public class User
{
	public int Id { get; set; }

	[Required]
	public string Name { get; set; } = string.Empty;

	[Required]
	[EmailAddress]
	public string Email { get; set; } = string.Empty;

	[Required]
	public string Department { get; set; } = string.Empty;
}
