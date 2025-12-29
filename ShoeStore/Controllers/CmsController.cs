using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoeStore.Dto.Cms;
using ShoeStore.Services;

namespace ShoeStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CmsController : ControllerBase
    {
        private readonly CmsService _cmsService;

        public CmsController(CmsService cmsService)
        {
            _cmsService = cmsService;
        }




        [HttpGet("navAndFooter")]
        public async Task<IActionResult> GetCmsNavAndFooterAsync()
        {

            try
            {
                var response = await _cmsService.GetCmsNavAndFooterAsync();

                if (response == null)
                {
                    return StatusCode(500, "Failed to get nav and footer.");
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while getting nav and footer", error = ex.Message });
            }
        }


        [HttpGet("landing")]
        public async Task<IActionResult> GetCmsLandingPageAsync()
        {

            try
            {
                var response = await _cmsService.GetCmsLandingPageAsync();

                if (response == null)
                {
                    return StatusCode(500, "Failed to get landing.");
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while getting landing", error = ex.Message });
            }
        }


        //[HttpGet("active")]
        //public async Task<IActionResult> GetCmsActiveProfileAsync()
        //{

        //    try
        //    {
        //        var response = await _cmsService.GetCmsActiveProfileAsync();

        //        if (response == null)
        //        {
        //            return StatusCode(500, "Failed to get active CMS profile.");
        //        }

        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "An error occurred while getting the active profile", error = ex.Message });
        //    }
        //}




        [HttpGet("profiles")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetCmsProfilesAsync()
        {

            try
            {
                var response = await _cmsService.GetCmsProfilesAsync();

                if (response == null)
                {
                    return StatusCode(500, "Failed to get CMS profiles.");
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while getting CMS profiles", error = ex.Message });
            }
        }



        [HttpGet("{profileId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetCmsProfileByIdAsync(int profileId)
        {
            try
            {
                if (profileId <=0)
                {
                    return BadRequest(profileId);
                }

                var response = await _cmsService.GetCmsProfileByIdAsync(profileId);

                if (response == null)
                {
                    return StatusCode(500, "Failed to get CMS profile.");
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while getting profile", error = ex.Message });
            }
        }



        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CreateCmsProfileAsync(CmsProfileDto profileToCreate)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _cmsService.CreateCmsProfileAsync(profileToCreate);

                if (response == null)
                {
                    return StatusCode(500, "Failed to create CMS profile.");
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating profile", error = ex.Message });
            }

        }

        [HttpPost("activate/{profileId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ActivateCmsProfileAsync(int profileId)
        {
            try
            {
                if (profileId <= 0)
                {
                    return BadRequest(profileId);
                }

                var response = await _cmsService.ActivateCmsProfileAsync(profileId);

                if (response == null)
                {
                    return StatusCode(500, "Failed to activate CMS profile.");
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while activating profile", error = ex.Message });
            }

        }


        [HttpPut]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateCmsProfileAsync(CmsProfileDto profileToUpdate)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var response = await _cmsService.UpdateCmsProfileAsync(profileToUpdate);
                if (response == null)
                {
                    return StatusCode(500, "Failed to update CMS profile.");
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating profile", error = ex.Message });
            }
        }

        [HttpDelete("{profileId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteCmsProfileAsync(int profileId)
        {
            try
            {
                if (profileId <= 0)
                {
                    return BadRequest(profileId);
                }

                var success = await _cmsService.DeleteCmsProfileAsync(profileId);
                if (!success)
                {
                    return StatusCode(500, "Failed to delete CMS profile.");
                }
                return Ok(new { message = "CMS profile deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting profile", error = ex.Message });
            }
        }
    }

}
