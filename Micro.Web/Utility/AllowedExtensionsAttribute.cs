using System.ComponentModel.DataAnnotations;

namespace Micro.Web.Utility;

/// <summary>
/// Custom validation attribute to ensure that a file has one of the allowed extensions.
/// </summary>
public class AllowedExtensionsAttribute : ValidationAttribute
{
	private readonly string[] _extensions;

	/// <summary>
	/// Initializes a new instance of the <see cref="AllowedExtensionsAttribute"/> class with the specified extensions.
	/// </summary>
	/// <param name="extensions">Array of allowed file extensions.</param>
	public AllowedExtensionsAttribute(string[] extensions)
	{
		_extensions = extensions;
	}

	/// <summary>
	/// Validates the passed object to check if its file extension is among the allowed extensions.
	/// </summary>
	/// <param name="value">The object to validate.</param>
	/// <param name="validationContext">Provides context about the validation operation, such as the object and property being validated.</param>
	/// <returns>A <see cref="ValidationResult"/> instance. Returns Success if valid, otherwise returns an error message.</returns>
	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		if (value is IFormFile file)
		{
			var extension = Path.GetExtension(file.FileName);
			if (!_extensions.Contains(extension.ToLower()))
			{
				return new ValidationResult("This photo extension is not allowed!");
			}
		}

		return ValidationResult.Success;
	}
}