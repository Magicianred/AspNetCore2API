using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using FormPdfEasy.Entities;
using FormPdfEasy.Interfaces;
using FormPdfEasy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FormPdfEasy.Controllers
{
    [Produces("application/json")]
    [Route("api/user")]
    [Authorize]
    public class UserController : Controller
    {
        private ILogger<UserController> _logger;
        private IUserRepository _userRepository;

        public UserController(ILogger<UserController> logger, 
            IUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        [HttpGet]
        [Authorize(Policy = "SuperUsers")]
        public IActionResult Get()
        {
            _logger.LogInformation("Start: Get Users");
            
            var users = _userRepository.GetUsers();

            _logger.LogInformation("End: Get Users");

            return Ok(users);
        }
    }
}