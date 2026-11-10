using System;

namespace JwtHelper.Core.Types
{
    [Serializable]
    internal class NoClaimsFoundInToken() : Exception("The JWT did not have any claims or claims were invalid!");
}