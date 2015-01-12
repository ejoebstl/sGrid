using System;


/// <summary>
/// This enumeration represents page permission roles. 
/// The roles can be treated like flags and combined,
/// for instance if an Admin and a CoinPartner role both should have 
/// access to a site. 
/// </summary>
[Flags]
public enum SiteRoles
{
    /// <summary>
    /// Specifies the administrator role, used for sGrid team members. 
    /// </summary>
    Admin = 1,
    /// <summary>
    /// Specifies the role of a coin partner.
    /// </summary>
    CoinPartner = 2,
    /// <summary>
    /// Specifies the role of a sponsor.
    /// </summary>
    Sponsor = 4,
    /// <summary>
    /// Specifies the role of a user.
    /// </summary>
    User = 8
}