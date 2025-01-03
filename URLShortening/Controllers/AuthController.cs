using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using URLShortening.DTOs;
using URLShortening.Helpers;
using URLShortening.Data;
using URLShortening.Services;

namespace URLShortening.Controllers;

[ApiController]
[ApiVersion("1.0")] // Specify the API version
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController(
    IUserHelper userHelper,
    IMailService mailService,
    IConfiguration configuration,
    ICodeGeneratorHelper codeGeneratorHelper) :
    ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Index() => Ok(new { Message = "Everything is ok!" });

    [HttpPost("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] AuthDto authDto)
    {
        var user = await userHelper.FindUserByEmailAsync(authDto.Email);
        if (user is not null)
        {
            return NotFound();
        }

        var newUser = new User()
            { Email = authDto.Email, UserName = authDto.Email };

        var result
            = await userHelper.CreateUserAsync(newUser, authDto.Password);
        if (!result)
        {
            return BadRequest();
        }

        var code = codeGeneratorHelper.GenerateConfirmationCode(newUser
            .Email, configuration["CodeKey"]);
        var mailResult = await mailService.SendEmailAsync(newUser.Email,
            "Confirm your email",
            $"<p>Your confirmation code is: {code}</p>");
        if (mailResult.IsSuccess)
        {
            return Ok();
        }

        return BadRequest();
    }

    [HttpPost("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail(
        [FromBody] EmailConfirmationDto confirmationDto)
    {
        var user = await userHelper.FindUserByEmailAsync(confirmationDto.Email);
        if (user == null)
        {
            return NotFound();
        }

        var actualCode = codeGeneratorHelper.GenerateConfirmationCode(user
            .Email, configuration["CodeKey"]);

        if (actualCode != confirmationDto.ConfirmationCode)
        {
            return BadRequest();
        }

        var token = await userHelper.GenerateEmailConfirmationTokenAsync(user);
        await userHelper.ConfirmEmailAsync(user, token);

        return Ok();
    }

    [HttpPost("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] AuthDto authDto)
    {
        var user = await userHelper.FindUserByEmailAsync(authDto
            .Email);

        if (user is null  || user is { EmailConfirmed: false })
        {
            return NotFound();
        }

        var confirmPassword = await userHelper.CheckPasswordAsync(user,
            authDto.Password);
        if (!confirmPassword)
        {
            return BadRequest();
        }

        var key = configuration["JWT:Key"] ??
                  throw new ArgumentNullException("JWT:Key",
                      "JWT:Key cannot be null.");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

        var credentials
            = new SigningCredentials(securityKey,
                SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, user.Email!)
        };

        var token = new JwtSecurityToken(
            issuer: configuration["JWT:Issuer"],
            audience: configuration["JWT:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(10),
            signingCredentials: credentials);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);


        return Ok(new
        {
            AccessToken = jwt,
            Expiration = (token.ValidTo - DateTime.Now).TotalSeconds,
            TokenType = "bearer",
            UserId = user.Id,
            user.UserName
        });
    }
}
