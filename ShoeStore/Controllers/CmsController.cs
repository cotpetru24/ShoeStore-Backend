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
                    return StatusCode(500, "Failed to get nav and footer.");

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
                    return StatusCode(500, "Failed to get landing.");

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while getting landing", error = ex.Message });
            }
        }

        [HttpGet("profiles")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetCmsProfilesAsync()
        {
            try
            {
                var response = await _cmsService.GetCmsProfilesAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while getting CMS profiles", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetCmsProfileByIdAsync(int id)
        {
            try
            {
                var response = await _cmsService.GetCmsProfileByIdAsync(id);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while getting CMS profile", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CreateCmsProfileAsync([FromBody] CmsProfileDto dto)
        {
            try
            {
                var response = await _cmsService.CreateCmsProfileAsync(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating CMS profile", error = ex.Message });
            }
        }

        [HttpPut]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateCmsProfileAsync([FromBody] CmsProfileDto dto)
        {
            try
            {
                var response = await _cmsService.UpdateCmsProfileAsync(dto);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating CMS profile", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteCmsProfileAsync(int id)
        {
            try
            {
                var result = await _cmsService.DeleteCmsProfileAsync(id);
                if (!result)
                    return NotFound(new { message = "CMS profile not found" });

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting CMS profile", error = ex.Message });
            }
        }

        [HttpPost("activate/{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ActivateCmsProfileAsync(int id)
        {
            try
            {
                var response = await _cmsService.ActivateCmsProfileAsync(id);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while activating CMS profile", error = ex.Message });
            }
        }
    }

}
