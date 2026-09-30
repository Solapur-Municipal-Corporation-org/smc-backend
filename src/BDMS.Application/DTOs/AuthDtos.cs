namespace BDMS.Application.DTOs;

public class LoginRequestDto
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public string? UserName { get; set; }
    public string? Role { get; set; }
    public string? Dept { get; set; }
    public string? RedirectTo { get; set; } // mirrors legacy's role-based redirect target
    public DateTime? ExpiresAt { get; set; }
}
