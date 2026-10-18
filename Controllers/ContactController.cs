using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using UmbracoDemo.Models.Contact;

namespace UmbracoDemo.Controllers;

/// <summary>
/// The one write-capable, non-Delivery-API endpoint in this feature (Constitution Principle II's
/// named exception; contracts/contact-submission.md). Deliberately a plain ASP.NET Core Web API
/// controller, not an Umbraco surface/render controller — no MVC view is ever rendered from it.
/// </summary>
[ApiController]
[Route("api/contact")]
public class ContactController : ControllerBase
{
    private const int SeedingUserId = Constants.Security.SuperUserId;

    private readonly IContentService _contentService;

    public ContactController(IContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpPost]
    public IActionResult Submit([FromBody] ContactSubmissionRequest request)
    {
        if (ModelState.IsValid is false)
        {
            var errors = ModelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(new ContactSubmissionValidationErrorResponse { Errors = errors });
        }

        IContent? container = FindSubmissionsContainer();
        if (container is null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ContactSubmissionValidationErrorResponse
            {
                Errors = new Dictionary<string, string[]>
                {
                    ["_"] = ["Contact submissions storage is not ready yet. Try again shortly."],
                },
            });
        }

        IContent submission = _contentService.Create(request.Name, container, "contactSubmission", SeedingUserId);
        submission.SetValue("email", request.Email);
        submission.SetValue("subject", request.Subject);
        submission.SetValue("message", request.Message);

        _contentService.Save(submission, SeedingUserId);
        _contentService.Publish(submission, ["*"], SeedingUserId);

        return StatusCode(StatusCodes.Status201Created, new ContactSubmissionSuccessResponse
        {
            SubmittedAt = submission.CreateDate,
        });
    }

    private IContent? FindSubmissionsContainer()
    {
        IContent? siteRoot = _contentService.GetRootContent().FirstOrDefault(c => c.ContentType.Alias == "siteRoot");
        if (siteRoot is null)
        {
            return null;
        }

        IContent? contactUs = _contentService.GetPagedChildren(siteRoot.Id, 0, 500, out _)
            .FirstOrDefault(c => c.ContentType.Alias == "contactUsPage");
        if (contactUs is null)
        {
            return null;
        }

        return _contentService.GetPagedChildren(contactUs.Id, 0, 500, out _)
            .FirstOrDefault(c => c.ContentType.Alias == "contactSubmissionsContainer");
    }
}
