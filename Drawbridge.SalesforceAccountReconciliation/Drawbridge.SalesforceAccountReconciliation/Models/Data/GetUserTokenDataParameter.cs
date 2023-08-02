// -----------------------------------------------------------------------
// <copyright file="GetUserTokenDataParameter.cs" company="Drawbridge Partners, LLC">
// Copyright (c) Drawbridge Partners, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Drawbridge.SalesforceAccountReconciliation.Models.Data
{
    /// <summary>
    /// Contains data used to add a <see cref="GetUserTokenDataParameter" />.
    /// </summary>
    public class GetUserTokenDataParameter
    {
#pragma warning disable SA1300
        /// <summary>
        /// Gets or sets the JWT token.
        /// </summary>
        public string _jwt { get; set; }
#pragma warning restore SA1300
    }
}