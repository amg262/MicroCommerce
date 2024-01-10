using System.ComponentModel.DataAnnotations;

namespace Micro.Services.ProductAPI.Models;

public class Product
{
	[Key] public int ProductId { get; set; }
	[Required] public string Name { get; set; }
	[Range(1, 1000)] public double Price { get; set; }
	public string Description { get; set; }
	public string CategoryName { get; set; }
	public string ImageUrl { get; set; }

	public override string ToString()
	{
		return
			$"{nameof(ProductId)}: {ProductId}, {nameof(Name)}: {Name}, {nameof(Price)}: {Price}, {nameof(Description)}: " +
			$"{Description}, {nameof(CategoryName)}: {CategoryName}, {nameof(ImageUrl)}: {ImageUrl}";
	}
}