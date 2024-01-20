﻿namespace Micro.Services.OrderAPI.Utility;

public static class SD
{
	public const string RoleAdmin = "ADMIN";
	public const string RoleCustomer = "CUSTOMER";

	public const string Status_Pending = "Pending";
	public const string Status_Approved = "Approved";
	public const string Status_ReadyForPickup = "ReadyForPickup";
	public const string Status_Completed = "Completed";
	public const string Status_Refunded = "Refunded";
	public const string Status_Cancelled = "Cancelled";
	public const string Status_Succeeded = "Succeeded";

	public static string ProductAPIBase { get; set; }
	public static string CouponAPIBase { get; set; }
}