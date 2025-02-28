using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class NeedPermissionAttribute(string permission) : AuthorizeAttribute(permission);
