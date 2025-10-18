using Microsoft.Extensions.Configuration;
using BCrypt.Net;
using PodcastService.Common.AppConfigurations.Bcrypt;
using PodcastService.Common.AppConfigurations.Bcrypt.interfaces;


namespace PodcastService.BusinessLogic.Helpers.AuthHelpers
{
    public class BcryptHelper
    {
        private readonly IBcryptConfig _bcryptConfig;
        private readonly int _saltRounds;

        public BcryptHelper(IBcryptConfig bcryptConfig)
        {
            _bcryptConfig = bcryptConfig;
            _saltRounds = _bcryptConfig.SALT_ROUNDS;
        }

        public string HashPassword(string password)
        {
            try
            {
                return BCrypt.Net.BCrypt.HashPassword(password, _saltRounds);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error PodcastService password: " + ex.Message);
                throw new Exception("Could not hash password");
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error verifying password: " + ex.Message);
                throw new Exception("Could not verify password");
            }
        }
    }
}
