namespace SMC.Master.Application.DTOs.Application;

public record ApplicationDto(
    Guid Id,
    Guid ServiceId,
    Guid CitizenId,
    string Status,
    DateTime SubmittedAt,
    DateTime UpdatedAt
);

public record UpdateApplicationStatusDto(string Status);
