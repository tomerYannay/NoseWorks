using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyFirstMvcApp.Data;
using MyFirstMvcApp.Models;
using System.Linq;
using System.Threading.Tasks;
using MyFirstMvcApp.Services;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace MyFirstMvcApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;
        private readonly ILogger<UserController> _logger;

        public UserController(ApplicationDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender, IMapper mapper, ILogger<UserController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _mapper = mapper;
            _logger = logger;
        }

        // POST: api/User/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmEmail", "User", new { userId = user.Id, token }, Request.Scheme);

                // Send email
                await _emailSender.SendEmailAsync(user.Email, "Confirm your email", $"Please confirm your email by clicking here: {confirmationLink}");

                return CreatedAtAction("GetUser", new { id = user.Id }, user);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }


        // GET: api/User/ConfirmEmail
        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return BadRequest("Invalid email confirmation request.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return Ok("Email confirmed successfully.");
            }

            return BadRequest("Email confirmation failed.");
        }

        // POST: api/User/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Model state is invalid.");
                return BadRequest(ModelState);
            }

            if (model == null)
            {
                _logger.LogError("Login model is null.");
                return BadRequest("Invalid login request.");
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogError("User not found.");
                return BadRequest("User not found.");
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("Login successful.");
                return Ok("Login successful.");
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return BadRequest("User account locked out.");
            }

            _logger.LogWarning("Invalid login attempt. Reason: {Reason}", result.ToString());
            return Unauthorized("Invalid login attempt.");
        }

        // POST: api/User/ForgotPassword
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "User", new { token, email = user.Email }, Request.Scheme);

            // Send email
            await _emailSender.SendEmailAsync(user.Email, "Reset Password", $"Please reset your password by clicking here: {resetLink}");

            return Ok("Password reset link has been sent to your email.");
        }

        // POST: api/User/ResetPassword
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("Password has been reset successfully.");
        }

        // GET: api/User
        [HttpGet]
        public IActionResult GetUsers()
        {
            return Ok(_context.Users.ToList());
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        // POST: api/User
        [HttpPost]
        public async Task<IActionResult> PostUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetUser", new { id = user.UserId }, user);
        }

        // PUT: api/User/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.UserId)
            {
                return BadRequest();
            }

            _context.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
