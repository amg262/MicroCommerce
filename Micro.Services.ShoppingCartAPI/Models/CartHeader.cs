using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Micro.Services.ShoppingCartAPI.Models;

public class CartHeader
{
	[Key] public int CartHeaderId { get; set; }
	public string? UserId { get; set; }
	public string? CouponCode { get; set; }
	[NotMapped] public double Discount { get; set; }
	[NotMapped] public double CartTotal { get; set; }

	public override string ToString()
	{
		return $"{nameof(CartHeaderId)}: {CartHeaderId}, {nameof(UserId)}: {UserId}, {nameof(CouponCode)}: " +
		       $"{CouponCode}, {nameof(Discount)}: {Discount}, {nameof(CartTotal)}: {CartTotal}";
	}
}