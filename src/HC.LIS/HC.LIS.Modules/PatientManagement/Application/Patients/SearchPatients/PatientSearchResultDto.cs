namespace HC.LIS.Modules.PatientManagement.Application.Patients.SearchPatients;

public class PatientSearchResultDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? DocumentId { get; set; }
    public string Status { get; set; } = string.Empty;
}
