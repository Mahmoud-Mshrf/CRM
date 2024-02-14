﻿using CRM.Core.Dtos;
using CRM.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly IAuthService _authService;
        public UserProfileController(IUserProfileService userProfileService, IAuthService authService)
        {
            _userProfileService = userProfileService;
            _authService = authService;
        }
        [HttpPut("update-name")]
        public async Task<IActionResult> UpdateName([FromBody] UpdateNameDto dto)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var result = await _userProfileService.UpdateNameAsync(email, dto);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
        [HttpPut("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var result = await _userProfileService.UpdatePasswordAsync(email, dto);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
        [HttpPut("update-email")]
        public async Task<IActionResult> UpdateEmail([FromForm] string NewEmail )
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var result = await _userProfileService.UpdateEmailAsync(email, NewEmail);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [AllowAnonymous]
        [HttpGet("ConfirmNewEmail")]
        public async Task<IActionResult> ConfirmNewEmail(string Id,string NewEmail,string Token)
        {
            var result = await _authService.ConfirmNewEmailAsync(Id,NewEmail,Token);
            if (!result.IsAuthenticated)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

    }
}