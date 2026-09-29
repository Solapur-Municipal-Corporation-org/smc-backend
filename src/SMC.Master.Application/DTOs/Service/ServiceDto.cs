namespace SMC.Master.Application.DTOs.Service;

public record ServiceDto(
    Guid Id,
    string NameEn,
    string NameMr,
    bool IsIntegrated,
    string? IntegrationKey,
    List<string> RequiredDocuments
);
