using System.IdentityModel.Tokens;
using Microsoft.IdentityModel.Claims;
using Microsoft.IdentityModel.Protocols.WSTrust;
using Microsoft.IdentityModel.SecurityTokenService;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Common
{
    public abstract class SecureTokenProviderBase
    {
        protected abstract SigningCredentials GetSigningCredentials();

        protected abstract IClaimsIdentity GetOutputClaimsIdentity();

        protected abstract EncryptingCredentials GetEncryptingCredentials();

        protected abstract Lifetime GetTokenLifeTime();

        protected abstract string SerializeToken(SecurityToken token);

        protected abstract string GetAppliesToAddress();

        protected abstract string GetIssuerName();

        protected abstract SecurityTokenHandler GetTokenHandler();

        protected abstract bool IsEncrypted();

        public virtual SecurityTokenDescriptor GetSecurityTokenDescriptor()
        {
            return new SecurityTokenDescriptor
            {
                AppliesToAddress = GetAppliesToAddress(),
                Lifetime = GetTokenLifeTime(),
                TokenIssuerName = GetIssuerName(),
                SigningCredentials = GetSigningCredentials(),
                EncryptingCredentials = (IsEncrypted()) ? GetEncryptingCredentials() : null,
                Subject = GetOutputClaimsIdentity()
            };
        }

        public virtual string Issue()
        {
            var handler = GetTokenHandler();
            var descriptor = GetSecurityTokenDescriptor();
            SecurityToken token = handler.CreateToken(descriptor);
            if (IsEncrypted())
            {
                EncryptedSecurityToken encryptedToken = new EncryptedSecurityToken(token, GetEncryptingCredentials());
                return SerializeToken(encryptedToken);
            }
            return SerializeToken(token);
        }
    }
}
