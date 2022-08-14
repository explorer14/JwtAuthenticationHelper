using System;

namespace JwtHelper.Core.Types
{
    [Serializable]
    internal class NoClaimsFoundInToken : Exception
    {
        private const string ERROR_MESSAGE = "The JWT did not have any claims or claims were invalid!";

        public NoClaimsFoundInToken() : base(ERROR_MESSAGE)
        {
        }
    }
}