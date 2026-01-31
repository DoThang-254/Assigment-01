using BusinessLogic.Dto;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Presentation.ViewModels.Auth;

namespace Presentation.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    public class accountController : ODataController
    {
        private readonly ISystemAccountService _systemAccountService;
        public accountController(ISystemAccountService systemAccountService) => _systemAccountService = systemAccountService;


        [EnableQuery]
        //[HttpGet]
        public IQueryable<SystemAccount> Get() => _systemAccountService.GetAccounts();

        [EnableQuery]
        public IActionResult Get([FromODataUri] short key)
        {
            try { 
                var result = _systemAccountService.GetAccountById(key);
                return Ok(result);
            }
            catch
            {
                return BadRequest("Something went wrong");
            }
        }


        public IActionResult Post([FromBody] CreateAccountRequest newAccount)
        {
            try
            {
                _systemAccountService.AddAccount(newAccount);
                return Ok("Account created successfully");
            }
            catch (Exception ex)
            {
                return BadRequest("Something went wrong");
            }
        }

        public IActionResult Put([FromRoute] short key , [FromBody] UpdateProfileRequestDto request)
        {
            try
            {
                _systemAccountService.UpdateAccount(key , request);
                return Ok("Account updated successfully");
            }
            catch
            {
                return BadRequest("Something went wrong");
            }
        }


        //[HttpPatch("admin-reset-password")]
        [HttpPatch]
        public IActionResult Patch([FromBody] AdminResetPasswordRequest request)
        {
            try
            {
                _systemAccountService.AdminResetPassword(
                    request.AdminEmail,
                    request.AdminPassword,
                    request.TargetAccountId,
                    request.NewPassword
                );

                return Ok("Password changed successfully");
            }
            catch
            {
                return BadRequest("Something went wrong");
            }
        }


        [HttpDelete]
        public IActionResult Delete(short key)
        {
            bool isDeleted = _systemAccountService.DeleteAccount(key);
            if (!isDeleted)
            {
                return BadRequest("Cannot delete account with associated news articles.");
            }
            return Ok("Account deleted successfully.");
        }

    }
}
