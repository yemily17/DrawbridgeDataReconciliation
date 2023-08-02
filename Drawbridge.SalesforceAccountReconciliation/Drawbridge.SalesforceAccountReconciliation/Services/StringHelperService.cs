// -----------------------------------------------------------------------
// <copyright file="StringHelperService.cs" company="Drawbridge Partners, LLC">
// Copyright (c) Drawbridge Partners, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Drawbridge.SalesforceAccountReconciliation.Services
{
    /// <summary>
    /// Helper functions for string parsing
    /// </summary>
    public class StringHelperService
    {
        /// <summary>
        /// Creates a new dictionary that replaces each key with new organization name from website or email
        /// </summary>
        /// <param name="searchDict">Dictionary to grab data from</param>
        /// <param name="websiteIndex">Column index of each organization's website</param>
        /// <param name="emailIndex">Column index of each organization's email</param>
        /// <returns>New dictionary with same values as searchDict but keys replaced with names parsed from website or email</returns>
        public static Dictionary<string, List<string>> ToFuzzy(Dictionary<string, List<string>> searchDict, int websiteIndex, int emailIndex)
        {
            var timer = new Stopwatch();
            double[] timearr = { 0.0, 0.0, 0.0 };
            var newDict = new Dictionary<string, List<string>>();
            var fuzzyDict = new Dictionary<string, List<string>>();
            foreach (KeyValuePair<string, List<string>> org in searchDict)
            {
                timer.Start();
                string websiteDomainName = ParseDomainNameFromWebsite(org.Value[websiteIndex]);
                timer.Stop();
                timearr[0] = timearr[0] + timer.ElapsedMilliseconds;
                timer.Restart();
                timer.Start();
                string emailDomainName = ParseDomainNameFromEmail(org.Value[emailIndex]);
                timer.Stop();
                timearr[1] = timearr[1] + timer.ElapsedMilliseconds;
                timer.Restart();
                if (!string.IsNullOrWhiteSpace(websiteDomainName))
                {
                    newDict.TryAdd(websiteDomainName, org.Value);
                }

                if (!string.IsNullOrWhiteSpace(emailDomainName))
                {
                    newDict.TryAdd(emailDomainName, org.Value);
                }

                /*else
                {
                    fuzzyDict.Add(org.Key, org.Value);
                }*/
            }

            Console.WriteLine($"Website time {timearr[0]}");
            Console.WriteLine($"Email time {timearr[1]}");
            return newDict;
        }

        /// <summary>
        /// Finds all words common to string1 and string2
        /// </summary>
        /// <param name="string1">first string to compare</param>
        /// <param name="string2">second string to compare</param>
        /// <returns>List of all words found in string1 and string2</returns>
        public static List<string> WordMatches(string string1, string string2)
        {
            return string1.Split().Intersect(string2.Split()).ToList();
        }

        /// <summary>
        /// Attempts to parse organization name from email
        /// </summary>
        /// <param name="email">email to parse</param>
        /// <returns>name parsed from email, or empty string if no email exists/name cannot be parsed</returns>
        public static string ParseDomainNameFromEmail(string email)
        {
            var emailSpan = email.AsSpan();
            if (emailSpan.Length == 0)
            {
                return string.Empty;
            }

            try
            {
                emailSpan = emailSpan[(emailSpan.IndexOf("@") + 1) ..];
                return emailSpan[..emailSpan.IndexOf(".")].ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Attempts to parse organization name from website
        /// </summary>
        /// <param name="website">website to parse</param>
        /// <returns>name parsed from website, or empty string if no website exists/name cannot be parsed</returns>
        public static string ParseDomainNameFromWebsite(string website) // make internal methods private move to another class file
        {
            int periodCount = 0;
            var websiteSpan = website.AsSpan();
            if (websiteSpan.Length == 0)
            {
                return string.Empty;
            }

            foreach (char ch in websiteSpan)
            {
                if (ch == '.')
                {
                    periodCount++;
                }
            }

            int endIndex;
            int startIndex;
            if (periodCount < 2)
            {
                if (websiteSpan.IndexOf("://") == -1)
                {
                    startIndex = 0;
                }
                else
                {
                    startIndex = websiteSpan.IndexOf("://") + 3;
                }

                endIndex = websiteSpan.IndexOf(".");
            }
            else
            {
                startIndex = websiteSpan.IndexOf(".") + 1;
                endIndex = websiteSpan.Slice(startIndex).IndexOf(".") + startIndex;
            }

            try
            {
                return websiteSpan.Slice(startIndex, endIndex - startIndex).ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
