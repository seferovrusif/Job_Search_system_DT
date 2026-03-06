using AutoMapper;
using JobSearch.Business.DTOs.AuthDTOs;
using JobSearch.Business.Exceptions.AppUser;
using JobSearch.Business.ExternalServices.Interfaces;
using JobSearch.Business.Services.Interfaces;
using JobSearch.Core.Entities;
using JobSearch.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Web;

namespace JobSearch.Business.Services.Implements
{
    public class AuthService:IAuthService
    {
        UserManager<AppUser> _userManager { get; }
        ITokenService _tokenService { get; }
        IMapper _mapper { get; }
        IEmailConfirmationService _emailService { get; }

        public AuthService(UserManager<AppUser> userManager, IMapper mapper, ITokenService tokenService, IEmailConfirmationService emailService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _tokenService = tokenService;
            _emailService = emailService;
        }
        public async Task CreateAsync(RegisterDTO dto)
        {
            AppUser user = _mapper.Map<AppUser>(dto);
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                ///TODO: belke strbuilderi liste deyisdim
                StringBuilder sb = new();
                foreach (var item in result.Errors)
                {
                    sb.Append(item.Description + " ");
                }
                throw new AppUserRegisterFailedException(sb.ToString().TrimEnd());
            }
            var roleResult = await _userManager.AddToRoleAsync(user, nameof(Roles.Member));
            if (!roleResult.Succeeded)
            {
                StringBuilder sb = new();
                foreach (var item in result.Errors)
                {
                    sb.Append(item.Description + " ");
                }
                throw new AppUserRegisterFailedException(sb.ToString().TrimEnd());
            }
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            byte[] tokenGeneratedBytes = Encoding.UTF8.GetBytes(token);
            var codeEncoded = WebEncoders.Base64UrlEncode(tokenGeneratedBytes);
            string confirmationLink = $"https://localhost:7105/api/Auths/ConfirmEmail?userId={user.Id}&token={codeEncoded}";
            string body = $"<h3>Welcome!</h3><p>Please <a href='{confirmationLink}'>click here</a> to confirm.</p>";
            await _emailService.SendEmailAsync(user.Email, "Confirm Account", body);
        }
        

        public async Task<TokenDTO> Login(LoginDTO dto)
        {
            AppUser User;
            if (dto.UserNameOrEmail.Contains("@"))
            {
                User= await _userManager.FindByEmailAsync(dto.UserNameOrEmail);
            }
            else
            {
                User = await _userManager.FindByNameAsync(dto.UserNameOrEmail);
            }
            if (User == null) throw new PasswordOrUserNameWrongException();
            if (!await _userManager.IsEmailConfirmedAsync(User))
            {
                throw new Exception("Email not confirmed. Please check your inbox.");
            }
            var result =await _userManager.CheckPasswordAsync(User, dto.Password);
            if (!result) throw new PasswordOrUserNameWrongException();
            string Role = (await _userManager.GetRolesAsync(User)).First();
            return _tokenService.CreateToken(new TokenItemsDTO
            {
                role = Role,
                user = User
            });
        }
        public async Task ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("User not found");
            var codeDecodedBytes = WebEncoders.Base64UrlDecode(token);
            var codeDecoded = Encoding.UTF8.GetString(codeDecodedBytes);
            var result = await _userManager.ConfirmEmailAsync(user, codeDecoded);
            if (!result.Succeeded)
            {
                var error = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Confirmation failed: {error}");
            }
        }


    }
}
