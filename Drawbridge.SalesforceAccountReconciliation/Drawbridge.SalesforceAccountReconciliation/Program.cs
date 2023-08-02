// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Drawbridge Partners, LLC">
// Copyright (c) Drawbridge Partners, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Drawbridge.WebApi;

namespace Drawbridge.SalesforceAccountReconciliation
{
    /// <summary>
    /// Entrypoint class for the application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point for the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>An integer return code.</returns>
        public static int Main(string[] args)
        {
            return Bootstrapper.RunApplication(args, typeof(Startup));
        }
    }
}
