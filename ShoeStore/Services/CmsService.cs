using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Cms;

namespace ShoeStore.Services
{
    public class CmsService
    {
        private readonly ShoeStoreContext _context;

        public CmsService(ShoeStoreContext context)
        {
            _context = context;
        }

        internal async Task<List<CmsStoredProfileDto>> GetCmsProfilesAsync()
        {
            var testProfile = new List<CmsStoredProfileDto>();


            return testProfile;
        }


        internal async Task<CmsProfileDto> GetCmsActiveProfileAsync()
        {

            //return the active cms profile from the database
            var testProfile = new CmsProfileDto();


            return testProfile;
        }

        internal async Task<CmsProfileDto> GetCmsProfileByIdAsync(int profileId)
        {
            var testProfile = new CmsProfileDto();


            return testProfile;
        }

        internal async Task<CmsProfileDto> CreateCmsProfileAsync(CmsProfileDto profileToCreate)
        {
            var testProfile = new CmsProfileDto();


            return testProfile;
        }

        internal async Task<CmsProfileDto> ActivateCmsProfileAsync(int profileId)
        {
            var testProfile = new CmsProfileDto();


            return testProfile;
        }

        internal async Task<CmsProfileDto> UpdateCmsProfileAsync(CmsProfileDto profileToUpdate)
        {
            var testProfile = new CmsProfileDto();


            return testProfile;
        }

        internal async Task<bool> DeleteCmsProfileAsync(int profileId)
        {
            throw new NotImplementedException();
        }


    }
}
