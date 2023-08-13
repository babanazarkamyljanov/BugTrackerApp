namespace BugTracker.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if(context.User == null)
        {
            return Task.CompletedTask;
        }

        var hasPermission = context.User.HasClaim("Permission", requirement.Permission);
        if(hasPermission)
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
