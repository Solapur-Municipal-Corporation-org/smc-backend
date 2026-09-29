namespace SMC.Master.Application.DTOs.Citizen;

public record CitizenDto(
    Guid Id,
    string FirstName,
    string? MiddleName,
    string LastName,
    DateOnly Dob,
    string Gender,
    string AadhaarNumber,
    string MobileNumber,
    string Email
);
