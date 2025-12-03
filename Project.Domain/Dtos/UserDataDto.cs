namespace Project.Domain.Dtos
{
    /// <summary>
  /// DTO para transferir datos básicos del usuario sin dependencias de Identity
    /// </summary>
    public class UserDataDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
        public string Identification { get; set; } = string.Empty;
    }
}