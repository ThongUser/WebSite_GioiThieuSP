using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace WebSite_GioiThieuSP.Helpers
{
    /// <summary>
    /// Helpers dùng chung cho các view giao diện Karl
    /// (Views/Product/index, shop, product-details).
    /// </summary>
    public static class ProductViewHelper
    {
        /// <summary>
        /// "Áo Khoác Nam" -> "ao-khoac-nam".
        /// Isotope lọc theo class CSS nên tên danh mục phải được chuẩn hoá thành slug hợp lệ.
        /// </summary>
        public static string Slug(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "khac";
            }

            var normalized = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in normalized)
            {
                // Bỏ dấu: chỉ giữ lại ký tự gốc, bỏ các dấu thanh tổ hợp.
                if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(ch);
                }
                else if (sb.Length > 0 && sb[sb.Length - 1] != '-')
                {
                    sb.Append('-');
                }
            }

            var slug = sb.ToString().Trim('-');
            return string.IsNullOrEmpty(slug) ? "khac" : slug;
        }

        /// <summary>
        /// Ảnh trong DB có thể là "/images/xxx.jpg" (do admin upload) hoặc rỗng.
        /// Phải đi qua Url.Content vì dấu "~" chỉ được Razor thay thế khi giá trị
        /// viết trực tiếp trong .cshtml, không thay thế được với biểu thức C#.
        /// </summary>
        public static string ResolveImage(IUrlHelper url, string? image, int index)
        {
            var path = string.IsNullOrWhiteSpace(image)
                ? $"~/client/img/product-img/product-{((index % 12) + 1)}.jpg"
                : image.Trim();

            if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            return url.Content(path.StartsWith("~/") ? path : "~/" + path.TrimStart('/')) ?? path;
        }

        /// <summary>
        /// DB có thể lưu nhiều ảnh cách nhau bởi dấu phẩy.
        /// Lấy ảnh đầu tiên làm ảnh chính.
        /// </summary>
        public static string? GetFirstImage(string? image)
        {
            if (string.IsNullOrWhiteSpace(image)) return null;
            return image.Split(',').FirstOrDefault()?.Trim();
        }

        /// <summary>Định dạng giá 2 chữ số, khớp kiểu hiển thị của template Karl.</summary>
        public static string Price(decimal value) => value.ToString("N2");
    }
}