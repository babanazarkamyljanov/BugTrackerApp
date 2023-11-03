using BugTracker.Authorization;
using BugTracker.Models.DTOs;
using BugTracker.ViewModels.ManageAccount;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace BugTracker.Controllers;

public class ManageController : Controller
{
    private readonly UserManager<User> userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly SignInManager<User> signInManager;
    private readonly ILogger logger;
    private readonly ApplicationDbContext context;
    private readonly IWebHostEnvironment hostEnvironment;
    private readonly IAuthorizationService authorizationService;

    public ManageController(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        SignInManager<User> signInManager,
        ILoggerFactory loggerFactory,
        ApplicationDbContext context,
        IWebHostEnvironment hostEnvironment,
        IAuthorizationService authorizationService)
    {
        this.userManager = userManager;
        _roleManager = roleManager;
        this.signInManager = signInManager;
        this.logger = loggerFactory.CreateLogger<ManageController>();
        this.context = context;
        this.hostEnvironment = hostEnvironment;
        this.authorizationService = authorizationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index1(string id, CancellationToken ct = default)
    {
        User? user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            throw new ArgumentException("User by id wasn't found", nameof(id));
        }

        string? organization = await context.Organizations
            .Where(o => o.Id == user.OrganizationId)
            .Select(o => o.Name)
            .FirstOrDefaultAsync(ct);
        if ( organization == null)
        {
            throw new ArgumentException("Organization wasn't found", nameof(id));
        }

        IList<string>? roles = await userManager.GetRolesAsync(user);
        if (roles == null)
        {
            throw new ArgumentException("Roles for user wasn't found", nameof(id));
        }

        if(user.AvatarPhoto != null)
        {
            ViewBag.ProfilePhotoSrc = "data:image/png;base64," + Convert.ToBase64String(user.AvatarPhoto, 0, user.AvatarPhoto.Length);
        }

        EditAccountDTO dto = new EditAccountDTO()
        {
            Id = id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.UserName,
            Email = user.Email,
            ProfilePhotoInBytes = user.AvatarPhoto,
            Organization = organization,
            RoleName = roles[0],
            PhoneNumber = user.PhoneNumber,
            Address = ""
        };

        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Index1(EditAccountDTO dto, CancellationToken ct = default)
    {
        if (dto.ProfilePhoto == null || dto.ProfilePhoto.Length == 0)
        {
            throw new ArgumentException("No photo is uploaded", nameof(dto.ProfilePhoto));
        }

        User? user = await userManager.FindByIdAsync(dto.Id);
        if (user == null)
        {
            throw new ArgumentException("User by id wasn't found", nameof(dto.Id));
        }

        try
        {
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;

            if (await userManager.IsInRoleAsync(user, dto.RoleName) == false)
            {
                await userManager.AddToRoleAsync(user, dto.RoleName);
            }

            using (var image = await Image.LoadAsync<Rgba32>(dto.ProfilePhoto.OpenReadStream()))
            {
                //image.Mutate(x => x.Resize(300, 300));

                using (var memoryStream = new MemoryStream())
                {
                    await image.SaveAsJpegAsync(memoryStream);
                    memoryStream.Position = 0;
                    user.AvatarPhoto = memoryStream.ToArray();
                }
            }

            IdentityResult? result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index1), new { dto.Id });
            }
            else
            {
                return View(dto);
            }
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An while editing account");
        }
    }

    // GET: /Manage/Index
    [HttpGet]
    public async Task<IActionResult> Index(string id, ManageMessageId? message = null)
    {
        var isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.AccountManageOperations.View);
        if (!isAuthorized.Succeeded)
        {
            return LocalRedirect("/Account/AccessDenied");
        }

        ViewData["userId"] = id;
        ViewData["StatusMessage"] =
            message == ManageMessageId.ChangePasswordSuccess ? "Password has been changed."
            : message == ManageMessageId.ChangeEmailSuccess ? "Email has been changed"
            : message == ManageMessageId.ChangeEmailFail ? "Error changing email."
            : message == ManageMessageId.ChangeUsernameFail ? "Error changing username."
            : message == ManageMessageId.ChangeAvatarSuccess ? "Avatar has been changed."
            : message == ManageMessageId.Error ? "An error has occurred."
            : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
            : message != null ? message
            : "";
        var user = await userManager.FindByIdAsync(id);

        if (user == null)
        {
            throw new ArgumentException("User by id wasn't found", nameof(id));
        }

        var model = new IndexViewModel
        {
            Id = user.Id.ToString(),
            Username = user.UserName,
            PhoneNumber = user.PhoneNumber
        };
        return View(model);
    }

    // POST: /Manage/Index
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Index")]
    public async Task<IActionResult> AddPhoneNumber(IndexViewModel model)
    {
        var isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.AccountManageOperations.ChangeEmail);
        if (!isAuthorized.Succeeded)
        {
            return LocalRedirect("/Account/AccessDenied");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.FindByIdAsync(model.Id);
        if (user == null)
        {
            return NotFound();
        }

        var code = await userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
        var result = await userManager.ChangePhoneNumberAsync(user, model.PhoneNumber, code);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(Index), new { id = model.Id, Message = ManageMessageId.AddPhoneSuccess });
        }

        return RedirectToAction(nameof(Index), new { id = model.Id, Message = ManageMessageId.Error });
    }

    // GET: /Manage/ChangeEmail
    [HttpGet]
    public async Task<IActionResult> ChangeEmail(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{id}'");
        }
        var currentUser = await GetCurrentUserAsync();

        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.AccountManageOperations.ChangeEmail);
        if (!isAuthorized.Succeeded)
        {
            return LocalRedirect("/Account/AccessDenied");
        }

        ChangeEmailViewModel model = new()
        {
            Id = id,
            Email = user.Email
        };

        return View(model);
    }

    // POST: /Manage/ChangeEmail
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeEmail(string id, ChangeEmailViewModel model)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{id}'");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }
        if (user.Email == model.NewEmail)
        {
            ModelState.AddModelError(nameof(model.NewEmail), "New email should be different.");
            return View(model);
        }
        else
        {
            var code = await userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await userManager.ChangeEmailAsync(user, model.NewEmail, code);
            if (!result.Succeeded)
            {
                return RedirectToAction(nameof(Index), new { id, Message = ManageMessageId.ChangeEmailFail });
            }

            // In our UI email and user name are one and the same, so when we update the email
            // we need to update the user name.
            var setUserNameResult = await userManager.SetUserNameAsync(user, model.NewEmail);
            if (!setUserNameResult.Succeeded)
            {
                return RedirectToAction(nameof(Index), new { id, Message = ManageMessageId.ChangeUsernameFail });
            }

            return RedirectToAction(nameof(Index), new { id, Message = ManageMessageId.ChangeEmailSuccess });
        }
    }

    // GET: /Manage/ChangePassword
    [HttpGet]
    public async Task<IActionResult> ChangePassword(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{id}'");
        }

        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.AccountManageOperations.ChangePassword);
        if (!isAuthorized.Succeeded)
        {
            return LocalRedirect("/Account/AccessDenied");
        }

        ViewData["userId"] = id;
        ChangePasswordViewModel model = new()
        {
            Id = id
        };
        return View(model);
    }

    // POST: /Manage/ChangePassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string id, ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", id);
        }
        var user = await userManager.FindByIdAsync(id);
        if (user != null)
        {
            var result = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                logger.LogInformation(3, "User changed their password successfully.");
                return RedirectToAction(nameof(Index), new { id, Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }
        return RedirectToAction(nameof(Index), new { id, Message = ManageMessageId.Error });
    }

    // GET: /Manage/ChangeAvatar
    [HttpGet]
    public async Task<IActionResult> ChangeAvatar(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{id}'");
        }

        ViewData["userId"] = id;
        if (user.AvatarPhoto != null)
        {
            ViewBag.UserAvatar = "data:image/png;base64," + Convert.ToBase64String(user.AvatarPhoto, 0, user.AvatarPhoto.Length);
        }

        ChangeAvatarViewModel model = new()
        {
            Id = id,
            AvatarPhotoBytes = user.AvatarPhoto
        };
        return View(model);
    }

    // POST: /Manage/ChangeAvatar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeAvatar(string id, ChangeAvatarViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{id}'");
        }

        using (var memoryStream = new MemoryStream())
        {
            await model.AvatarPhoto.CopyToAsync(memoryStream);
            if (memoryStream.Length == 0)
            {
                ModelState.AddModelError(model.AvatarPhoto.Name, "file is empty");
                return View(model);
            }
            var fileContent = memoryStream.ToArray();
            user.AvatarPhoto = fileContent;
            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index), new { id, Message = ManageMessageId.ChangeAvatarSuccess });
            }
        }

        // if we got this far, something is wrong
        return RedirectToAction(nameof(Index), new { id, Message = ManageMessageId.Error });
    }

    // GET: /Manage/PersonalData
    public async Task<IActionResult> PersonalData(string id)
    {
        ViewData["id"] = id;
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{id}'");
        }

        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.AccountManageOperations.Delete);
        if (!isAuthorized.Succeeded)
        {
            return LocalRedirect("/Account/AccessDenied");
        }

        return View();
    }

    // GET: /Manage/Delete
    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{id}'");
        }
        DeleteAccountViewModel model = new()
        {
            Id = id,
            RequirePassword = await userManager.HasPasswordAsync(user)
        };
        return View(model);
    }

    // POST: /Manage/Delete
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirm(DeleteAccountViewModel model)
    {
        var user = await userManager.FindByIdAsync(model.Id);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{model.Id}'.");
        }

        if (model.RequirePassword)
        {
            if (!await userManager.CheckPasswordAsync(user, model.Password))
            {
                ModelState.AddModelError(string.Empty, "Incorrect password.");
                return View(model);
            }
        }

        var userAssignedBugs = await context.Bugs.Where(b => b.Assignee.Id == model.Id).ToListAsync();

        bool isCurrentUser = false;
        if (user == await GetCurrentUserAsync())
        {
            isCurrentUser = true;
        }

        // set null to AssignedUserId of User Bugs
        if (userAssignedBugs.Count != 0)
        {
            foreach (var bug in userAssignedBugs)
            {
                bug.Assignee = null;
                context.Update(bug);
            }
            await context.SaveChangesAsync();
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Unexpected error occurred deleting user.");
        }

        if (isCurrentUser)
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Account", "Login");
        }
        else
        {
            return RedirectToAction("Index", "Organization");
        }
    }

    #region Helpers
    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    public enum ManageMessageId
    {
        AddPhoneSuccess,
        ChangePasswordSuccess,
        ChangeEmailSuccess,
        ChangeEmailFail,
        ChangeUsernameFail,
        ChangeAvatarSuccess,
        Error
    }
    private Task<User> GetCurrentUserAsync()
    {
        return userManager.GetUserAsync(HttpContext.User);
    }
    #endregion Helpers
}
