using LibraryBookLendingSystem_8._0_.Models.DTOs.Books;

namespace LibraryBookLendingSystem_8._0_.Validators
{
    public record FieldError(string Field, string Message);

    /// <summary>One set of book rules, shared by the API endpoints and the CSV import (limits mirror the SQL schema).</summary>
    public static class BookValidator
    {
        public static void Normalize(BookUpsertDto b)
        {
            b.Isbn = Clean(b.Isbn);
            b.Title = (b.Title ?? "").Trim();
            b.Author = (b.Author ?? "").Trim();
            b.Publisher = Clean(b.Publisher);
            b.Category = (b.Category ?? "").Trim();
            b.ShelfLocation = Clean(b.ShelfLocation);
            b.AccessUrl = Clean(b.AccessUrl);
        }

        public static List<FieldError> Validate(BookUpsertDto b)
        {
            var errors = new List<FieldError>();

            Required(errors, "Title", b.Title, 250);
            Required(errors, "Author", b.Author, 150);
            Required(errors, "Category", b.Category, 80);
            MaxLength(errors, "Publisher", b.Publisher, 150);
            MaxLength(errors, "ShelfLocation", b.ShelfLocation, 50);

            if (b.Isbn != null)
            {
                if (b.Isbn.Length > 20) errors.Add(new("Isbn", "ISBN must be at most 20 characters."));
                else if (!IsValidIsbn(b.Isbn)) errors.Add(new("Isbn", "ISBN must have 10 or 13 digits (hyphens allowed)."));
            }

            if (b.PublicationYear is < 1400 or > 2100)
                errors.Add(new("PublicationYear", "Publication year must be between 1400 and 2100."));

            if (b.IsDigitalResource && b.AccessUrl == null)
                errors.Add(new("AccessUrl", "A digital resource needs an AccessUrl."));

            if (b.AccessUrl != null)
            {
                if (b.AccessUrl.Length > 500)
                    errors.Add(new("AccessUrl", "AccessUrl must be at most 500 characters."));
                else if (!Uri.TryCreate(b.AccessUrl, UriKind.Absolute, out var uri)
                         || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                    errors.Add(new("AccessUrl", "AccessUrl must be a valid http or https link."));
            }

            return errors;
        }

        private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        private static void Required(List<FieldError> errors, string field, string value, int max)
        {
            if (value.Length == 0) errors.Add(new(field, $"{field} is required."));
            else if (value.Length > max) errors.Add(new(field, $"{field} must be at most {max} characters."));
        }

        private static void MaxLength(List<FieldError> errors, string field, string? value, int max)
        {
            if (value != null && value.Length > max) errors.Add(new(field, $"{field} must be at most {max} characters."));
        }

        private static bool IsDigit(char c) => c is >= '0' and <= '9';

        private static bool IsValidIsbn(string isbn)
        {
            var s = isbn.Replace("-", "").Replace(" ", "");
            if (s.Length == 13) return s.All(IsDigit);
            if (s.Length == 10) return s[..9].All(IsDigit) && (IsDigit(s[9]) || s[9] is 'X' or 'x');
            return false;
        }
    }
}
