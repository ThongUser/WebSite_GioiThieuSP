using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSite_GioiThieuSP.Models;

public class Order
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress, StringLength(256)]
    public string? Email { get; set; }

    [Required, StringLength(200)]
    [Column("Address")]
    public string ShippingAddress { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Note { get; set; }

    [Column("CreatedDate", TypeName = "datetime")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required, StringLength(50)]
    public string Status { get; set; } = OrderStatuses.Pending;

    public ICollection<OrderDetail> Items { get; set; } = new List<OrderDetail>();
}

public static class OrderStatuses
{
    public const string Pending = "Chờ xác nhận";
    public const string Processing = "Đang xử lý";
    public const string Shipped = "Đang giao";
    public const string Completed = "Hoàn thành";
    public const string Cancelled = "Đã hủy";

    public static readonly string[] All =
    [
        Pending,
        Processing,
        Shipped,
        Completed,
        Cancelled
    ];
}