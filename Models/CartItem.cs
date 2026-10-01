using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSite_GioiThieuSP.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Image { get; set; }
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        // Tính thành tiền cho sản phẩm này
        [NotMapped]
        public decimal Total => Price * Quantity;
    }
}