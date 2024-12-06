namespace Infrastructure.Authorization;

internal sealed class PermissionProvider
{
    public Task<HashSet<string>> GetForUserIdAsync(Guid userId)
    {
        if (Guid.Empty.Equals(userId))
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }
        // TODO: Here you'll implement your logic to fetch permissions.
        HashSet<string> permissionsSet = [];

        return Task.FromResult(permissionsSet);
    }
}
