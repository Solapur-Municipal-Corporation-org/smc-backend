using System.ComponentModel.DataAnnotations;
using CitizenPortal.Api.Models;

namespace CitizenPortal.Api.Modules.Wtd;

/// <summary>
/// WORKED EXAMPLE of the "custom table" integration path (see Modules/_TEMPLATE
/// for the blank template and INTEGRATION_GUIDE.md for the full explanation).
///
/// Why this needs its own table instead of just Application.FormDataJson: the
/// citizen's original form only captures the request. This record is filled in
/// *afterwards* by a Water Tax Department inspector during their site visit —
/// it's staff-entered follow-up data, not part of the citizen's submission, so
/// it doesn't belong in the generic FormDataJson blob. It stays 1:1 linked to
/// the Application that triggered the inspection.
/// </summary>
public class WaterConnectionInspection
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ApplicationId { get; set; }
    public Application? Application { get; set; }

    public DateTime InspectionDate { get; set; }

    [MaxLength(150)]
    public string InspectorName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string MeterNumber { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Remarks { get; set; } = string.Empty;
}
