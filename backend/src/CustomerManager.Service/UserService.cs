using CustomerManager.Model.Common;
using CustomerManager.Model.Result;
using CustomerManager.Repository.Interfaces;
using CustomerManager.Service.Interfaces;
using MongoDB.Driver;
using System;
using System.Net;
using System.Threading.Tasks;

namespace CustomerManager.Service
{
    public class UserService : IUserService
    {
        private readonly IMongoRepository<User> _userRepository;

        public UserService(IMongoRepository<User> repository)
        {
            _userRepository = repository;
        }

        public async Task<Result<bool>> Validate(User user)
        {
            if (user == null)
                return new Result<bool>(false, HttpStatusCode.BadRequest,
                    new ArgumentNullException("user cannot be null!"));

            try
            {
                var filter = Builders<User>.Filter.Where(u => u.Login == user.Login && u.Password == user.Password);

                var userRepository = await _userRepository.FindAsync(filter);
                if (userRepository == null)
                    return new Result<bool>(false, HttpStatusCode.NotFound, new Exception("invalid user!"));

                return new Result<bool>(true, HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return new Result<bool>(false, HttpStatusCode.InternalServerError,
                    new Exception($"could not validate user: {e.Message}"));
            }
        }
    }
}