using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CitizenPortal.Api.Department.Entities;

// ---------------------------------------------------------------------------
// Legacy lookup masters, generated from tbl_scripts.txt (uploaded SQL script).
// Table names, column names and lengths are kept exactly as in that script.
// ---------------------------------------------------------------------------

[Table("tbl_bloodgrp")]
public class BloodGroupMaster
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(10)]
    public string BldGrpCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? BldGrpName { get; set; }
    [MaxLength(80)]
    public string? BldGrpNameRL { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

[Table("tbl_bank")]
public class BankMaster
{
    [Column("Sr.No")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(10)]
    public string BankCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? BankName { get; set; }
    [MaxLength(80)]
    public string? BankNameRL { get; set; }

    /// <summary>Branch code, unique within a bank (BankCode). One row = one branch.</summary>
    [MaxLength(20)]
    public string? BranchCode { get; set; }
    [MaxLength(150)]
    public string? BranchName { get; set; }
    [MaxLength(15)]
    public string? IfscCode { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

[Table("tbl_class")]
public class ClassMaster
{
    [Column("Sr.No")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(10)]
    public string ClassCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? ClassName { get; set; }
    [MaxLength(80)]
    public string? ClassNameRL { get; set; }
    public decimal? TotalSheets { get; set; }
    public decimal? ApprSheets { get; set; }
    public decimal? GoverQuota { get; set; }
    public decimal? ManagQuota { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

[Table("tbl_cast")]
public class CasteMaster
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(10)]
    public string CastCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? CastName { get; set; }
    [MaxLength(80)]
    public string? CastNameRL { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

[Table("tbl_ctr")]
public class CountryMaster
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(10)]
    public string ContryCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? ContryName { get; set; }
    [MaxLength(80)]
    public string? ContryNameRL { get; set; }
    [MaxLength(80)]
    public string? ContTelCode { get; set; }
    [MaxLength(80)]
    public string? PANDigits { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

[Table("tbl_city")]
public class CityMaster
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(10)]
    public string CityCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? CityName { get; set; }
    [MaxLength(80)]
    public string? CityNameRL { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

[Table("tbl_dst")]
public class DistrictMaster
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(10)]
    public string DestCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? DestName { get; set; }
    [MaxLength(80)]
    public string? DestNameRL { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

[Table("tbl_edu")]
public class EducationTypeMaster
{
    [Column("Sr.No")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(10)]
    public string EduCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? EduName { get; set; }
    [MaxLength(80)]
    public string? EduNameRL { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

[Table("tbl_gender")]
public class GenderMaster
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(10)]
    public string TitleCode { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? Title { get; set; }
    [MaxLength(10)]
    public string? TitleRL { get; set; }
    [MaxLength(50)]
    public string? GenderName { get; set; }
    [MaxLength(80)]
    public string? GenderNameRL { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

[Table("tbl_location")]
public class LocationMaster
{
    [Column("Sr.No")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(10)]
    public string LocCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? LocName { get; set; }
    [MaxLength(80)]
    public string? LocNameRL { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

[Table("tbl_mrs")]
public class MaritalStatusMaster
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(10)]
    public string MarrStaCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? MarrStaName { get; set; }
    [MaxLength(80)]
    public string? MarrStaNameRL { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

[Table("tbl_occu")]
public class OccupationMaster
{
    [Column("Sr.No")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(10)]
    public string OccupCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? OccupName { get; set; }
    [MaxLength(80)]
    public string? OccupNameRL { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

[Table("tbl_dc")]
public class RequiredDocumentMaster
{
    [Column("Sr.No")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(10)]
    public string DocCode { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? SerCode { get; set; }
    [MaxLength(50)]
    public string? DocName { get; set; }
    [MaxLength(80)]
    public string? DocNameRL { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

[Table("tbl_rel")]
public class ReligionMaster
{
    [Column("Sr.No")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(10)]
    public string ReligenCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? ReligenName { get; set; }
    [MaxLength(80)]
    public string? ReligenNameRL { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

[Table("tbl_relt")]
public class RelationTypeMaster
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(10)]
    public string RelCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Relation { get; set; }
    [MaxLength(80)]
    public string? RelationRL { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

[Table("tbl_state")]
public class StateMaster
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(10)]
    public string SatateCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? SatateName { get; set; }
    [MaxLength(80)]
    public string? SatateNameRL { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

[Table("tbl_tahl")]
public class TahsilMaster
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(10)]
    public string TahashilCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? TahashilName { get; set; }
    [MaxLength(80)]
    public string? TahashilNameRL { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

[Table("tbl_tit")]
public class TitleMaster
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(10)]
    public string TitleCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Title { get; set; }
    [MaxLength(80)]
    public string? TitleRL { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

[Table("tbl_ust")]
public class UserStatusMaster
{
    [Column("Sr.No")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(10)]
    public string UserCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? UserName { get; set; }
    [MaxLength(80)]
    public string? UserNameRL { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

[Table("tbl_ward")]
public class WardMaster
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(10)]
    public string WardCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? WardName { get; set; }
    [MaxLength(80)]
    public string? WardNameRL { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

[Table("tbl_zone")]
public class ZoneMaster
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(10)]
    public string ZoneCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? ZoneName { get; set; }
    [MaxLength(80)]
    public string? ZoneNameRL { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

[Table("tbl_org")]
public class OrgMaster
{
    [Column("Sr.No")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SrNo { get; set; }

    [Key]
    [MaxLength(15)]
    public string OrgCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? OrgName { get; set; }
    [MaxLength(100)]
    public string? OrgNameRL { get; set; }
    [MaxLength(15)]
    public string? HOCode { get; set; }
    [MaxLength(50)]
    public string? PrintHead { get; set; }
    [MaxLength(80)]
    public string? PrintHeadRL { get; set; }
    [MaxLength(20)]
    public string? OffFlatRoom { get; set; }
    [MaxLength(20)]
    public string? OffFloor { get; set; }
    [MaxLength(20)]
    public string? OffBuilding { get; set; }
    [MaxLength(20)]
    public string? OffBlock { get; set; }
    [MaxLength(20)]
    public string? OffStreet { get; set; }
    [MaxLength(20)]
    public string? OffLandmark { get; set; }
    [MaxLength(10)]
    public string? OffCityCode { get; set; }
    [MaxLength(10)]
    public string? OffTahashil { get; set; }
    [MaxLength(10)]
    public string? OffDistrict { get; set; }
    [MaxLength(10)]
    public string? OffStateCode { get; set; }
    [MaxLength(10)]
    public string? OffCountryCode { get; set; }
    [MaxLength(6)]
    public string? OffPINCode { get; set; }
    [MaxLength(200)]
    public string? OffAddress { get; set; }
    [MaxLength(80)]
    public string? OfficerName { get; set; }
    [MaxLength(10)]
    public string? OffTelNo1 { get; set; }
    [MaxLength(10)]
    public string? OffTelNo2 { get; set; }
    [MaxLength(10)]
    public string? OffFaxNo { get; set; }
    [MaxLength(20)]
    public string? OffEmailID { get; set; }
    [MaxLength(20)]
    public string? InternetURL { get; set; }
    [MaxLength(1)]
    public string? Defolt { get; set; }

    public string? FirmCode { get; set; }
    public string? ComName { get; set; }
    public string? TCPIP { get; set; }
    public string? MCompName { get; set; }
    public string? CUserSign { get; set; }
    public string? MUserSign { get; set; }
    public string? Hold { get; set; } = "N";
    public string? Canceled { get; set; } = "N";
    public string? Deleted { get; set; } = "N";
    public DateTime? UpdateDate { get; set; }
    public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? Remarks { get; set; }
}

