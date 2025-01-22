﻿using AutoMapper;
using GOVE.Infrastructure.Queries;
using GOVE.Models.Requests;
using GOVE.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using static GOVE.Models.Constants.Constants;
using System.Security.Claims;
using System.Text;
using static GOVE.Models.Responses.Response;
using GOVE.Models.Constants;
using GOVE.Infrastructure.Services.Identity_Services;

namespace GOVE.Admin.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IConfiguration _configuration;
        public AuthController(IConfiguration configuration, IMediator mediator, IMapper mapper, ILogger<AuthController> logger) : base(mediator, logger, mapper)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("GetLoginDetails")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK, Web.ContentType.Json)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest, Web.ContentType.Json)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError, Web.ContentType.Json)]
        public async Task<IActionResult> GetUserById(Models.Requests.LoginRequest loginRequest)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(loginRequest.UserName, nameof(loginRequest.UserName));
                ArgumentNullException.ThrowIfNull(loginRequest.Password, nameof(loginRequest.Password));
                var query = new GetLoginQuery.Query(loginRequest.UserName, loginRequest.Password);
                var user = await GoveMediator.Send(query);
                if (user == null)
                    return new BadRequestObjectResult(new Models.Responses.MessageResponse
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        Error = new ErrorResponse
                        {
                            Message = Models.Constants.Constants.Messages.INVALID_USER,
                        }
                    });
                var userToken = await IdentityServer4Client.LoginAsync(_configuration[Constants.IdentityServerConfigurationKey]!, loginRequest.UserName, loginRequest.Password);
                user.SessionExpireDate = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) + userToken.ExpiresIn;
                return Ok(new GoveResponse
                {
                    Status = Status.Success,
                    Message = user//new { User = user, Token = userToken.AccessToken, RefreshToken = userToken.RefreshToken }
                }); 

            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
    }
}
