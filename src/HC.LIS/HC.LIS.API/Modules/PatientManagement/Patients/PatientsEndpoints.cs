using HC.LIS.API.Common;
using HC.LIS.API.Modules.PatientManagement.Patients.AnonymizePatient;
using HC.LIS.API.Modules.PatientManagement.Patients.GetPatientDetails;
using HC.LIS.API.Modules.PatientManagement.Patients.RegisterPatient;
using HC.LIS.API.Modules.PatientManagement.Patients.SearchPatients;
using HC.LIS.API.Modules.PatientManagement.Patients.UpdatePatient;
using HC.LIS.Modules.PatientManagement.Application.Patients.GetPatientDetails;
using HC.LIS.Modules.PatientManagement.Application.Patients.SearchPatients;

namespace HC.LIS.API.Modules.PatientManagement.Patients;

internal static class PatientsEndpoints
{
    internal static RouteGroupBuilder MapPatientsEndpoints(this RouteGroupBuilder group)
    {
        group.WithTags("Patients");

        group.MapPost("", RegisterPatientEndpoint.Handle)
            .RequireAuthorization("PatientManagement")
            .WithName("RegisterPatient")
            .WithSummary("Register a new patient.")
            .Produces<CreatedIdResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("", SearchPatientsEndpoint.Handle)
            .RequireAuthorization("PatientManagement")
            .WithName("SearchPatients")
            .WithSummary("Search patients by name or document ID.")
            .Produces<IReadOnlyCollection<PatientSearchResultDto>>();

        group.MapGet("{id:guid}", GetPatientDetailsEndpoint.Handle)
            .RequireAuthorization("PatientManagement")
            .WithName("GetPatientDetails")
            .WithSummary("Get patient details by ID.")
            .Produces<PatientDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("{id:guid}", UpdatePatientEndpoint.Handle)
            .RequireAuthorization("PatientManagement")
            .WithName("UpdatePatient")
            .WithSummary("Update patient demographics.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("{id:guid}/anonymize", AnonymizePatientEndpoint.Handle)
            .RequireAuthorization("ITAdmin")
            .WithName("AnonymizePatient")
            .WithSummary("Irreversibly anonymize a patient record.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }
}
