using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMC.Infrastructure.Migrations;

public partial class RenamePrefixedTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameTable("AllocationProcesses", newName: "TR_AST_AllocationProcesses");
        migrationBuilder.RenameTable("ApplicationNumberConfigurations", newName: "MR_CFC_ApplicationNumberConfigurations");
        migrationBuilder.RenameTable("AuditLogs", newName: "TR_AST_AuditLogs");
        migrationBuilder.RenameTable("Calculations", newName: "TR_AST_Calculations");
        migrationBuilder.RenameTable("Complaints", newName: "TR_CFC_Complaints");
        migrationBuilder.RenameTable("DemandApplicationDocuments", newName: "TR_CFC_DemandApplicationDocuments");
        migrationBuilder.RenameTable("DemandApplications", newName: "TR_CFC_DemandApplications");
        migrationBuilder.RenameTable("DemandApplicationWorkflows", newName: "TR_CFC_DemandApplicationWorkflows");
        migrationBuilder.RenameTable("DemandApplicationZoneDocuments", newName: "TR_CFC_DemandApplicationZoneDocuments");
        migrationBuilder.RenameTable("DemandApplicationZoneWorkflows", newName: "TR_CFC_DemandApplicationZoneWorkflows");
        migrationBuilder.RenameTable("DemandBusinessTypeMasters", newName: "MR_CFC_DemandBusinessTypes");
        migrationBuilder.RenameTable("DemandServiceMasters", newName: "MR_CFC_DemandServices");
        migrationBuilder.RenameTable("DemandServiceRates", newName: "MR_CFC_DemandServiceRates");
        migrationBuilder.RenameTable("Documents", newName: "TR_AST_Documents");
        migrationBuilder.RenameTable("GalaTransferDocuments", newName: "TR_AST_GalaTransferDocuments");
        migrationBuilder.RenameTable("GalaTransferRates", newName: "MR_AST_GalaTransferRates");
        migrationBuilder.RenameTable("GalaTransfers", newName: "TR_AST_GalaTransfers");
        migrationBuilder.RenameTable("LandRegistrations", newName: "TR_AST_LandRegistrations");
        migrationBuilder.RenameTable("Leases", newName: "TR_AST_Leases");
        migrationBuilder.RenameTable("MasterData", newName: "MR_AST_MasterData");
        migrationBuilder.RenameTable("PermitConfigurations", newName: "MR_CFC_PermitConfigurations");
        migrationBuilder.RenameTable("PermitTerms", newName: "MR_CFC_PermitTerms");
        migrationBuilder.RenameTable("Properties", newName: "MR_AST_Properties");
        migrationBuilder.RenameTable("RecoveryCases", newName: "TR_AST_RecoveryCases");
        migrationBuilder.RenameTable("SchemeApplications", newName: "TR_AST_SchemeApplications");
        migrationBuilder.RenameTable("SmsEvents", newName: "TR_CFC_SmsEvents");
        migrationBuilder.RenameTable("SmsTemplates", newName: "MR_CFC_SmsTemplates");
        migrationBuilder.RenameTable("Users", newName: "MR_AST_Users");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameTable("MR_AST_Users", newName: "Users");
        migrationBuilder.RenameTable("MR_CFC_SmsTemplates", newName: "SmsTemplates");
        migrationBuilder.RenameTable("TR_CFC_SmsEvents", newName: "SmsEvents");
        migrationBuilder.RenameTable("TR_AST_SchemeApplications", newName: "SchemeApplications");
        migrationBuilder.RenameTable("TR_AST_RecoveryCases", newName: "RecoveryCases");
        migrationBuilder.RenameTable("MR_AST_Properties", newName: "Properties");
        migrationBuilder.RenameTable("MR_CFC_PermitTerms", newName: "PermitTerms");
        migrationBuilder.RenameTable("MR_CFC_PermitConfigurations", newName: "PermitConfigurations");
        migrationBuilder.RenameTable("MR_AST_MasterData", newName: "MasterData");
        migrationBuilder.RenameTable("TR_AST_Leases", newName: "Leases");
        migrationBuilder.RenameTable("TR_AST_LandRegistrations", newName: "LandRegistrations");
        migrationBuilder.RenameTable("TR_AST_GalaTransfers", newName: "GalaTransfers");
        migrationBuilder.RenameTable("MR_AST_GalaTransferRates", newName: "GalaTransferRates");
        migrationBuilder.RenameTable("TR_AST_GalaTransferDocuments", newName: "GalaTransferDocuments");
        migrationBuilder.RenameTable("TR_AST_Documents", newName: "Documents");
        migrationBuilder.RenameTable("MR_CFC_DemandServiceRates", newName: "DemandServiceRates");
        migrationBuilder.RenameTable("MR_CFC_DemandServices", newName: "DemandServiceMasters");
        migrationBuilder.RenameTable("MR_CFC_DemandBusinessTypes", newName: "DemandBusinessTypeMasters");
        migrationBuilder.RenameTable("TR_CFC_DemandApplicationZoneWorkflows", newName: "DemandApplicationZoneWorkflows");
        migrationBuilder.RenameTable("TR_CFC_DemandApplicationZoneDocuments", newName: "DemandApplicationZoneDocuments");
        migrationBuilder.RenameTable("TR_CFC_DemandApplicationWorkflows", newName: "DemandApplicationWorkflows");
        migrationBuilder.RenameTable("TR_CFC_DemandApplications", newName: "DemandApplications");
        migrationBuilder.RenameTable("TR_CFC_DemandApplicationDocuments", newName: "DemandApplicationDocuments");
        migrationBuilder.RenameTable("TR_CFC_Complaints", newName: "Complaints");
        migrationBuilder.RenameTable("TR_AST_Calculations", newName: "Calculations");
        migrationBuilder.RenameTable("TR_AST_AuditLogs", newName: "AuditLogs");
        migrationBuilder.RenameTable("MR_CFC_ApplicationNumberConfigurations", newName: "ApplicationNumberConfigurations");
        migrationBuilder.RenameTable("TR_AST_AllocationProcesses", newName: "AllocationProcesses");
    }
}
