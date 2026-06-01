namespace JobSearch.Business.DTOs.AuthDTOs
{
    public class VerifyTwoFactorDTO
    {
        public string UserNameOrEmail { get; set; }
        public string Code { get; set; }
    }
}
