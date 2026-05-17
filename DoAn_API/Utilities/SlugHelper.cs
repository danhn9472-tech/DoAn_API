using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DoAn_API.Utilities
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return string.Empty;

            // Xóa dấu tiếng Việt
            var normalizedString = title.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            var slug = stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();

            // Chỉ giữ lại chữ, số và khoảng trắng
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            // Biến khoảng trắng thành dấu gạch ngang
            slug = Regex.Replace(slug, @"\s+", "-").Trim('-');

            return slug;
        }
    }
}
