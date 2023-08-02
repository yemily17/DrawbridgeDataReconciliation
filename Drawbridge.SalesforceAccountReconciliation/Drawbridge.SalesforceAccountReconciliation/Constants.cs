// -----------------------------------------------------------------------
// <copyright file="Constants.cs" company="Drawbridge Partners, LLC">
// Copyright (c) Drawbridge Partners, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Drawbridge.SalesforceAccountReconciliation
{
    /// <summary>
    /// Constant values used by the SF account reconciliation library
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Column index of name field in Salesforce excel file
        /// </summary>
        public const int SfNameColumn = 74;

        /// <summary>
        /// Column index of name field in HFM excel file
        /// </summary>
        public const int SfWebsiteColumn = 126;

        /// <summary>
        /// Column index of name field in HFM excel file
        /// </summary>
        public const int SfEmailColumn1 = 84;

        /// <summary>
        /// Column index of name field in HFM excel file
        /// </summary>
        public const int SfEmailColumn2 = 35;

        /// <summary>
        /// Column index of name field in Preqin excel file
        /// </summary>
        public const int PreqinNameColumn = 1;

        /// <summary>
        /// Column index of name field in HFM excel file
        /// </summary>
        public const int HfmNameColumn = 1;

        /// <summary>
        /// Column index of name field in HFM excel file
        /// </summary>
        public const int PreqinWebsiteColumn = 8;

        /// <summary>
        /// Column index of name field in HFM excel file
        /// </summary>
        public const int PreqinEmailColumn = 9;

        /// <summary>
        /// Column index of name field in HFM excel file
        /// </summary>
        public const int HfmWebsiteColumn = 24;

        /// <summary>
        /// Column index of name field in HFM excel file
        /// </summary>
        public const int HfmEmailColumn = 25;

        /// <summary>
        /// HFM match file name
        /// </summary>
        public const string HfmMatchFileName = "HFMMatches.csv";

        /// <summary>
        /// Preqin match file name
        /// </summary>
        public const string PreqinMatchFileName = "PreqinMatches.csv";

        /// <summary>
        /// HFM close match file name
        /// </summary>
        public const string HfmCloseFileName = "HFMClose.csv";

        /// <summary>
        /// Preqin close match file name
        /// </summary>
        public const string PreqinCloseFileName = "PreqinClose.csv";

        /// <summary>
        /// No match file name
        /// </summary>
        public const string NoMatchFileName = "NoMatch.csv";

        /// <summary>
        /// Salesforce results file name
        /// </summary>
        public const string SfResultsFileName = "SFResults.csv";

        /// <summary>
        /// Standard header section for Salesforce data
        /// </summary>
        public const string SfStandardHeader = "\"SFID\",\"SF Name\",\"SF City\",\"SF State\"";
    }
}
