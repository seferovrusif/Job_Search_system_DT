using JobSearch.Core.Entities;

namespace JobSearch.Business.ExternalServices.Interfaces
{
    public interface IEmailConfirmationService
    {
        Task SendConfirmationEmailAsync(AppUser user, string confirmationLink);
    }
}
