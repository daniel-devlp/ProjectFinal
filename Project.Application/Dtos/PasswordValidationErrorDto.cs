using System.Collections.Generic;

namespace Project.Application.Dtos
{
    public class PasswordValidationErrorDto
    {
        public string ErrorCode { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}