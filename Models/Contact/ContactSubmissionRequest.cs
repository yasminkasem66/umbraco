using System.ComponentModel.DataAnnotations;

namespace UmbracoDemo.Models.Contact;

/// <summary>Request body for POST /api/contact (contracts/contact-submission.md).</summary>
public class ContactSubmissionRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(150, MinimumLength = 3)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Message { get; set; } = string.Empty;
}
