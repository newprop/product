﻿namespace Jamidar.Services
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user);
    }
}
