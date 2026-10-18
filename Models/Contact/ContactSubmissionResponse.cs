namespace UmbracoDemo.Models.Contact;

/// <summary>201 success body for POST /api/contact (contracts/contact-submission.md).</summary>
public class ContactSubmissionSuccessResponse
{
    public string Status { get; } = "success";

    public DateTime SubmittedAt { get; set; }
}

/// <summary>400 validation-error body for POST /api/contact (contracts/contact-submission.md).</summary>
public class ContactSubmissionValidationErrorResponse
{
    public string Status { get; } = "validationError";

    public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
}
