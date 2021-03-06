﻿using CustomerManager.Models.Entities;
using CustomerManager.Models.Helpers.Interfaces;
using CustomerManager.Models.Results;
using CustomerManager.Models.Transients;
using CustomerManager.Repositories.Interfaces;
using CustomerManager.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CustomerManager.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IMongoRepository<User> _userRepository;
        private readonly ISettings _settings;

        public AuthenticationService(IMongoRepository<User> userRepository, ISettings settings)
        {
            _userRepository = userRepository;
            _settings = settings;
        }

        public async Task<Result<AuthenticateResponse>> AuthenticateAsync(AuthenticateRequest authenticateRequest)
        {
            if (authenticateRequest == null)
                return new Result<AuthenticateResponse>(null, HttpStatusCode.BadRequest, new ArgumentNullException("authentication request can't be null!"));

            if (string.IsNullOrEmpty(authenticateRequest.Username))
                return new Result<AuthenticateResponse>(null, HttpStatusCode.BadRequest, new ArgumentException("username can't be null!"));

            if (string.IsNullOrEmpty(authenticateRequest.Password))
                return new Result<AuthenticateResponse>(null, HttpStatusCode.BadRequest, new ArgumentException("password can't be null!"));

            try
            {
                var filter = Builders<User>.Filter.Where(u => u.UserName == authenticateRequest.Username && u.Password == authenticateRequest.Password);
                var user = await _userRepository.FindAsync(filter);
                if (user == null)
                    return new Result<AuthenticateResponse>(null, HttpStatusCode.NotFound, new Exception("invalid user or password!"));

                var token = GenerateJwtToken(user);
                var authenticateResponse = new AuthenticateResponse(user, token);
                return new Result<AuthenticateResponse>(authenticateResponse, HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return new Result<AuthenticateResponse>(null, HttpStatusCode.InternalServerError, new Exception($"coud not authenticate: {e.Message}"));
            }
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_settings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id) }),
                Expires = DateTime.UtcNow.AddHours(12),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}