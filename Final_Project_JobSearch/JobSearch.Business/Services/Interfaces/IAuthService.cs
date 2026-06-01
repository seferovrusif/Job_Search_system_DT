using JobSearch.Business.DTOs.AuthDTOs;

namespace JobSearch.Business.Services.Interfaces
{
    public interface IAuthService
    {

        public Task<TokenDTO> Login(LoginDTO dto);
        public Task CreateAsync(RegisterDTO dto);
        public Task ConfirmEmailAsync(string userId, string token);
        public Task ResendConfirmationEmailAsync(string email);
    }
}
