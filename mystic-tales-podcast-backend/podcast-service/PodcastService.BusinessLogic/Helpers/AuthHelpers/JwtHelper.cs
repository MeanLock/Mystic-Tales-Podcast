using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PodcastService.Common.AppConfigurations.Jwt;
using System.Security.Cryptography.X509Certificates;
using PodcastService.Common.AppConfigurations.Jwt.interfaces;
using PodcastService.BusinessLogic.Helpers.DateHelpers;

namespace PodcastService.BusinessLogic.Helpers.AuthHelpers
{
    public class JwtHelper
    {
        private readonly IJwtConfig _jwtConfig;
        private readonly DateHelper _dateHelper;

        private readonly string _secretKey;
        private readonly string _privateKey;
        private readonly string _publicKey;

        public JwtHelper(IJwtConfig _jwtConfig, DateHelper dateHelper)
        {
            _secretKey = _jwtConfig.SecretKey;
            _privateKey = _jwtConfig.PrivateKey;
            _publicKey = _jwtConfig.PublicKey;
            this._jwtConfig = _jwtConfig;
            _dateHelper = dateHelper;
        }

        //public string  getPublicKey()
        //{
        //    return this._publicKeyKey;
        //}

        // Tạo JWT với Secret Key
        public string GenerateJWT_OneSecretKey(object payload, double expiresInMinutes, string? secretKey = null)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey ?? _secretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    claims: ConvertObjectToClaims(payload),
                    expires: _dateHelper.GetNowByAppTimeZone().AddMinutes(expiresInMinutes),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error generating JWT: " + ex.Message);
                throw new Exception("Could not generate JWT");
            }
        }

        // Tạo JWT với Public/Private Key
        public string GenerateJWT_TwoPublicPrivateKey(List<Claim> payload, double expiresInMinutes)
        {

            try
            {
                var rsa = RSA.Create();
                rsa.ImportFromPem(_privateKey.ToCharArray());

                // Sử dụng private key để ký token
                var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);

                var token = new JwtSecurityToken(
                    issuer: _jwtConfig.Issuer,
                    audience: _jwtConfig.Audience,
                    claims: payload,
                    expires: _dateHelper.GetNowByAppTimeZone().AddMinutes(expiresInMinutes),
                    signingCredentials: signingCredentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error generating JWT: " + ex.Message);
                throw new Exception("Could not generate JWT");
            }
        }

        // Giải mã JWT sử dụng Secret Key
        public ClaimsPrincipal DecodeToken_OneSecretKey(string token, string? secretKey = null)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey ?? _secretKey));
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true, // ✅ kiểm tra expired
                    ClockSkew = TimeSpan.Zero // không cho phép lệch thời gian
                };

                return new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
            }
            catch (SecurityTokenExpiredException)
            {
                throw new Exception("Token has expired");
            }
            catch (Exception)
            {
                throw new Exception("Invalid token");
            }
        }

        // Giải mã JWT sử dụng Public Key
        public ClaimsPrincipal DecodeToken_TwoPublicPrivateKey(string token)
        {

            try
            {
                var publicKey = _publicKey;
                var rsa = RSA.Create();
                rsa.ImportFromPem(publicKey);

                var key = new RsaSecurityKey(rsa);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                return new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid token");
            }
        }

        // Lấy token từ Header Authorization
        public static string ExtractAuthorizationHeaderToken(string authorizationHeader)
        {
            if (string.IsNullOrEmpty(authorizationHeader))
                throw new UnauthorizedAccessException("Authorization header is missing");

            if (!authorizationHeader.StartsWith("Bearer "))
                throw new UnauthorizedAccessException("Invalid authorization header format");

            return authorizationHeader.Replace("Bearer ", "");
        }

        // Chuyển object thành danh sách Claims
        private static Claim[] ConvertObjectToClaims(object payload)
        {
            if (payload is not IDictionary<string, object> dict)
                throw new ArgumentException("Payload must be a dictionary");

            return dict.Select(kv => new Claim(kv.Key, kv.Value?.ToString() ?? "")).ToArray();
        }

        public static T ClaimsPrincipalToObject<T>(ClaimsPrincipal user) where T : new()
        {
            if (user == null) return default;

            var obj = new T();
            var properties = typeof(T).GetProperties();

            foreach (var prop in properties)
            {
                var claim = user.FindFirst(prop.Name);
                if (claim != null)
                {
                    var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    object value;
                    if (targetType == typeof(Guid))
                    {
                        value = Guid.Parse(claim.Value);
                    }
                    else if (targetType.IsEnum)
                    {
                        value = Enum.Parse(targetType, claim.Value);
                    }
                    else
                    {
                        value = Convert.ChangeType(claim.Value, targetType);
                    }

                    prop.SetValue(obj, value);
                }
            }

            return obj;
        }
    }
}
