using CustomerManagementPractiseCS.ViewModels.Profile;

namespace CustomerManagementPractiseCS.Services.Interfaces
{
    public interface IProfileCompletenessService
    {
        void Fill(ProfileIndexViewModel viewModel);
    }
}
