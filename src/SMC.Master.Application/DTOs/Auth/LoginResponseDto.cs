namespace SMC.Master.Application.DTOs.Auth;

public record LoginResponseDto(string Token, string RefreshToken, string Role, DateTime ExpiresAt);
