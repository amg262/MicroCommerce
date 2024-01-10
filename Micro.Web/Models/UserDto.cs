namespace Micro.Web.Models;

public record UserDto
{
	public string Id { get; set; }
	public string Email { get; set; }
	public string Name { get; set; }
	public string PhoneNumber { get; set; }

	public override string ToString()
	{
		return $"{nameof(Id)}: {Id}, {nameof(Email)}: {Email}, {nameof(Name)}: " +
		       $"{Name}, {nameof(PhoneNumber)}: {PhoneNumber}";
	}
}