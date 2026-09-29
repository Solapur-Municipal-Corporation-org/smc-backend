using SMC.Master.Application.DTOs.Citizen;
using SMC.Master.Application.Validators;

namespace SMC.Master.Application.Tests;

public class RegisterCitizenValidatorTests
{
    [Fact]
    public void Rejects_InvalidAadhaarLength()
    {
        var validator = new RegisterCitizenValidator();
        var dto = new RegisterCitizenDto("Ravi", null, "Patil", new DateOnly(1990, 1, 1),
            "Male", "12345", "9876543210", "ravi@example.com", "Password123");

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }
}
