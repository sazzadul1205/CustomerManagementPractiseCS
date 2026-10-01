using CustomerManagementPractiseCS.Services.Interfaces;
using CustomerManagementPractiseCS.ViewModels.Profile;

namespace CustomerManagementPractiseCS.Services
{
    public class ProfileCompletenessService : IProfileCompletenessService
    {
        public void Fill(ProfileIndexViewModel viewModel)
        {
            int total = 7;
            int done = 0;

            // string.IsNullOrEmpty Check if the provided string is Null or Empty 
            if (!string.IsNullOrEmpty(viewModel.PhotoUrl))
            {
                done++;
            }
            if (!string.IsNullOrEmpty(viewModel.Summary))
            {
                done++;
            }
            if (viewModel.Addresses.Count > 0)
            {
                done++;
            }
            if (viewModel.Contacts.Count > 0)
            {
                done++;
            }
            if (viewModel.Educations.Count > 0)
            {
                done++;
            }
            if (viewModel.Experiences.Count > 0)
            {
                done++;
            }
            if (viewModel.SocialLinks.Count > 0)
            {
                done++;
            }

             // Convert to Percentage
            int percentage = (done * 100) / total;

            viewModel.CompletedSections = done;

            viewModel.TotalSections = total;
            viewModel.CompletenessPercentage = percentage;
            viewModel.CompletenessLabel = "Profile " + percentage + "% complete";
            viewModel.IsProfileComplete = percentage >= 100;

        }
    }
}
