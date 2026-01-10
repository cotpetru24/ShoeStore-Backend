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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CmsNavAndFooterDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCmsNavAndFooterAsync()
        {
            var response = await _cmsService.GetCmsNavAndFooterAsync();
            if (response == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to get nav and footer." });

            return Ok(response);
        }


        [HttpGet("landing")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CmsLandingPageDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCmsLandingPageAsync()
        {
            var response = await _cmsService.GetCmsLandingPageAsync();
            if (response == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to get landing." });

            return Ok(response);
        }


        [HttpGet("profiles")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CmsStoredProfileDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCmsProfilesAsync()
        {
            var response = await _cmsService.GetCmsProfilesAsync();
            return Ok(response);
        }


        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CmsProfileDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCmsProfileByIdAsync(int id)
        {
            var response = await _cmsService.GetCmsProfileByIdAsync(id);
            return Ok(response);
        }


        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CmsProfileDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCmsProfileAsync([FromBody] CmsProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _cmsService.CreateCmsProfileAsync(dto);
            return Ok(response);
        }


        [HttpPut]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CmsProfileDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCmsProfileAsync([FromBody] CmsProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _cmsService.UpdateCmsProfileAsync(dto);
            return Ok(response);
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCmsProfileAsync(int id)
        {
            var result = await _cmsService.DeleteCmsProfileAsync(id);
            if (!result)
                return NotFound(new { message = "CMS profile not found" });

            return Ok(result);
        }


        [HttpPost("activate/{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CmsProfileDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActivateCmsProfileAsync(int id)
        {
            var response = await _cmsService.ActivateCmsProfileAsync(id);
            return Ok(response);
        }
    }
}
