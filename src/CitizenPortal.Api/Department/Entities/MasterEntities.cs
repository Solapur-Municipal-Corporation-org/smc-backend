namespace CitizenPortal.Api.Department.Entities;

/// <summary>Country sheet.</summary>
public class Country
{
    public int CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public string? MobileCode { get; set; }

    public ICollection<State> States { get; set; } = new List<State>();
}

/// <summary>State sheet.</summary>
public class State
{
    public int StateId { get; set; }
    public string StateName { get; set; } = string.Empty;
    public int CountryId { get; set; }
    public Country? Country { get; set; }
    public string? Remarks { get; set; }

    public ICollection<District> Districts { get; set; } = new List<District>();
}

/// <summary>District sheet.</summary>
public class District
{
    public int DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public int StateId { get; set; }
    public State? State { get; set; }
    public string? Remarks { get; set; }

    public ICollection<Tahsil> Tahsils { get; set; } = new List<Tahsil>();
}

/// <summary>Tahsil sheet.</summary>
public class Tahsil
{
    public int TahsilId { get; set; }
    public string TahsilName { get; set; } = string.Empty;
    public int DistrictId { get; set; }
    public District? District { get; set; }
    public string? Remarks { get; set; }

    public ICollection<City> Cities { get; set; } = new List<City>();
}

/// <summary>City sheet.</summary>
public class City
{
    public int CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
    public int TahsilId { get; set; }
    public Tahsil? Tahsil { get; set; }
    public string? Remarks { get; set; }
}

/// <summary>Location sheet — standalone location list (e.g. wards/areas), per organization_Master.xlsx.</summary>
public class Location
{
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}

/// <summary>Address sheet — flat address record, fields exactly as in organization_Master.xlsx.</summary>
public class Address
{
    public int AddressId { get; set; }
    public int OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string? Location { get; set; }
    public string? CityName { get; set; }
    public string? CityCode { get; set; }
    public string? Tahsil { get; set; }
    public string? District { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? Pincode { get; set; }
}

/// <summary>Financial Year sheet.</summary>
public class FinancialYear
{
    public int FinancialYearId { get; set; }
    public string FinancialYearName { get; set; } = string.Empty; // e.g. "2026-27"
    public string? Remarks { get; set; }

    public ICollection<Holiday> Holidays { get; set; } = new List<Holiday>();
}

/// <summary>Holidays sheet.</summary>
public class Holiday
{
    public int HolidayId { get; set; } // Sr.No
    public int FinancialYearId { get; set; }
    public FinancialYear? FinancialYear { get; set; }
    public DateTime Date { get; set; }
    public string FestivalName { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
    public string? Remarks { get; set; }
}
