namespace SafeRouteAI.Domain.Entities;

public class ModelVersion
{
    public int ModelVersionID { get; set; }

    public string VersionLabel { get; set; } = string.Empty;

    public DateTime TrainingDate { get; set; }

    public string DatasetVersion { get; set; } = string.Empty;

    public string ModelPath { get; set; } = string.Empty;

    public string ValidationSummary { get; set; } = string.Empty;
}