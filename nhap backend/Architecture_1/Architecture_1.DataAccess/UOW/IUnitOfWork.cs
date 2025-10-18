using Architecture_1.DataAccess.Repositories;
using Architecture_1.DataAccess.Repositories.interfaces;

namespace Architecture_1.DataAccess.UOW;
public interface IUnitOfWork
{
    IAccountRepository AccountRepository { get; }
    IFacilityRepository FacilityRepository  { get; }
    IFacilityMajorRepository FacilityMajorRepository  { get; }
    IServiceRepository ServiceRepository  { get; }
    IFeedbackRepository FeedbackRepository  { get; }
    IFacilityItemAssignmentRepository FacilityItemAssignmentRepository  { get; }
    IAssigneeFacilityMajorAssignmentRepository AssigneeFacilityMajorAssignmentRepository  { get; }
    IReportRepository ReportRepository  { get; }
    IServiceAvailabilityRepository ServiceAvailabilityRepository  { get; }
    ITaskRequestRepository TaskRequestRepository  { get; }
    IServiceRequestRepository ServiceRequestRepository  { get; }
    IChatRepository ChatRepository  { get; }


    int Complete();
}
