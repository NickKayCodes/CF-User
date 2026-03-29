using CF_User.Data.TO.Create;
using CF_User.Data.TO.Delete;
using CF_User.Data.TO.Get;
using CF_User.Data.TO.Update;
using CF_User.Model.enums;


namespace CF_User.Services.User

{
    public interface IUserService
    {
        Task<CreateUserResponse> CreateUserAsync(string username, string email, string password, UserRole role);
        Task<GetByEmailResponse> GetUserByEmailAsync(string email);
        Task<DeleteUserResponse> DeleteUserByIdAsync(Guid id);
        Task<UpdateUserResponse> UpdateUserByIdAsync(Guid id, string? username, string? email, string? password, UserRole? role, List<Privilege>? privileges);
    }
}
