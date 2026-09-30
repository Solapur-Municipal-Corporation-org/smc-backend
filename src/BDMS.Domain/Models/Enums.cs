namespace BDMS.Domain.Models;

// Preserves legacy status string values exactly (VerifiedApplicationStatus, Payment_Made_Yes_No)
// so reporting logic and any legacy data import lines up 1:1.

public enum ApplicationStatus
{
    Pending,
    Approved,
    Rejected
}

public enum PaymentStatus
{
    No,   // Payment_Made_Yes_No = "No"
    Yes   // Payment_Made_Yes_No = "Yes"
}

public enum ApplicationType
{
    Birth,
    Death
}

public enum UserRole
{
    Citizen,
    Clerk,     // legacy "Operator"
    Archivist  // legacy "Abhilekhapal"
}

public enum Gender
{
    Male,
    Female,
    Other
}

public enum RegistrationType
{
    // ddlNavNondni in legacy — normal vs delayed/court-order registration
    Normal,
    DelayedWithCourtOrder,
    DelayedWithoutCourtOrder
}

/// <summary>
/// Legacy dropdowns (ddlAfterFifteenYears, ddlNavNondni) are literally "-- Select --" / "Yes" / "No".
/// Modeled as a 3-value enum so "not yet answered" is representable (legacy blocks submit on it).
/// </summary>
public enum YesNoSelect
{
    NotSelected,
    Yes,
    No
}

/// <summary>
/// ddlRecordFound on the officer verification screen (BDMS_Page.aspx) — the clerk's
/// cross-check result against physical archive / CRS / Mainet records. This is the field
/// that actually drives VerifiedApplicationStatus (see OfficerVerificationService), not a
/// simple Approve/Reject choice.
/// </summary>
public enum RecordFoundDecision
{
    Pending,       // legacy default -- the dropdown's literal pre-selected value, not a blank placeholder
    Yes,           // record found and verified -> Approved
    No,            // record not found -> Rejected
    Abhilekhapal,  // routed to the archivist for a physical-record search
    SubRegistrar   // routed to the Sub-Registrar (legacy: minimal-validation shortcut path)
}

/// <summary>ddlCRSMainet — which external government system the matched record came from.</summary>
public enum CrsMainetSource
{
    NotSelected,
    CRS,
    Mainet
}
