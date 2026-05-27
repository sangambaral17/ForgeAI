using ForgeAI.Application.Contracts;

namespace ForgeAI.Application.Interfaces;

public interface IUserService
{
    Task<UserResponse> GetDemoUserAsync();
}