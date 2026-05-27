using ForgeAI.Application.Contracts;
using ForgeAI.Application.Interfaces;

namespace ForgeAI.Infrastructure.Services;

public class UserService : IUserService
{
    public Task<UserResponse> GetDemoUserAsync()
    {
        var user = new UserResponse
        {
            Id = Guid.NewGuid(),
            Email = "admin@forgeai.dev",
            FullName = "ForgeAI Admin"
        };

        return Task.FromResult(user);
    }
}