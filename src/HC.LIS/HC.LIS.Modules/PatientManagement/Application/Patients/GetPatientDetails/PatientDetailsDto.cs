namespace HC.LIS.Modules.PatientManagement.Application.Patients.GetPatientDetails;

public class PatientDetailsDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? MothersFullName { get; set; }
    public string? DocumentId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public DateTime? AnonymizedAt { get; set; }
}
