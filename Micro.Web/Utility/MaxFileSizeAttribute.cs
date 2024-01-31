using System.ComponentModel.DataAnnotations;

namespace Micro.Web.Utility;

/// <summary>
/// Custom validation attribute to specify a maximum file size for IFormFile properties.
/// </summary>
public class MaxFileSizeAttribute : ValidationAttribute
{
	private readonly int _maxFileSize;

	/// <summary>
	/// Initializes a new instance of the <see cref="MaxFileSizeAttribute"/> class.
	/// </summary>
	/// <param name="maxFileSize">The maximum file size in megabytes.</param>
	public MaxFileSizeAttribute(int maxFileSize)
	{
		_maxFileSize = maxFileSize;
	}

	/// <summary>
	/// Determines whether the specified file is of an acceptable size as defined by the <see cref="MaxFileSizeAttribute"/>.
	/// </summary>
	/// <param name="value">The object to validate. Expected to be of type <see cref="IFormFile"/>.</param>
	/// <param name="validationContext">Describes the context in which the validation check is performed.</param>
	/// <returns><see cref="ValidationResult.Success"/> if the file size is valid; otherwise, an error message.</returns>
	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		if (value is IFormFile file)
		{
			if (file.Length > (_maxFileSize * 2048 * 2048))
			{
				return new ValidationResult($"Maximum allowed file size is {_maxFileSize} MB.");
			}
		}

		return ValidationResult.Success;
	}
}