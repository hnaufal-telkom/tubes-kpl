using MainLibrary;
using System.Collections.Generic;

namespace WebAPI.Service
{
    public class LeaveRequestService
    {
        private readonly ILeaveRequestRepository _leaveRepository;

        public LeaveRequestService(ILeaveRequestRepository leaveRepository)
        {
            _leaveRepository = leaveRepository;
        }

        public IEnumerable<LeaveRequest> GetPendingLeaveRequests() =>
            _leaveRepository.GetPendingLeaveRequests();

        public LeaveRequest GetLeaveById(string id) =>
            _leaveRepository.GetLeaveById(id);

        public IEnumerable<LeaveRequest> GetByUserId(string userId) =>
            _leaveRepository.GetByUserId(userId);
    }
}
