// -----------------------------------------------------------------------
// <copyright file="ReconciliationReportHelperService.cs" company="Drawbridge Partners, LLC">
// Copyright (c) Drawbridge Partners, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Drawbridge.SalesforceAccountReconciliation;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Drawbridge.SalesforceAccountReconciliation.Services
{
    /// <summary>
    /// Service for performing send grid health check requests.
    /// </summary>
    public class ReconciliationReportHelperService
    {
        /// <summary>
        /// Converts an excel file into a DataTable
        /// </summary>
        /// <param name="fileName">name of file to parse</param>
        /// <returns>DataTable representation of excel file</returns>
        public static DataTable BuildDataTable(IFormFile fileName)
        {
            using (var stream = fileName.OpenReadStream())
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var conf = new ExcelDataSetConfiguration
                    {
                        UseColumnDataType = true,
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = false
                        }
                    };
                    var dataset = reader.AsDataSet(conf).Tables[0];
                    return dataset;
                }
            }
        }

        /// <summary>
        /// Converts DataTable object to dictionary
        /// </summary>
        /// <param name="dataTable">DataTable to convert into dictionary containing all orgs and associated data</param>
        /// <param name="nameColumnIndex">Column in DataTable that contains each organization's name</param>
        /// <returns>Dictionary with key for organization name and value for all associated data</returns>
        public static Dictionary<string, List<string>> ToDictionary(DataTable dataTable, int nameColumnIndex)
        {
            var orgs = new Dictionary<string, List<string>>(); // TODO: match type in matchfile
            string[] legalNames = { "llc", "llp", "lp", "inc", "ltd", "ventures", "venture", "management", "investment", "managers", "capital", "partners", "investments", "incorporated", "management", "group", "the", "financial", "advisors", "holdings", "sas" };
            string punctuation = "./`\"':;,.[]{}()&\r\n";
            for (var i = 1; i < dataTable.Rows.Count; i++)
            {
                var row = dataTable.Rows[i];
                string name = row[nameColumnIndex].ToString().ToLower();
                foreach (char letter in punctuation)
                {
                    name = name.Replace(letter.ToString(), string.Empty);
                }

                name = string.Join(
                    " ",
                    name
                        .Split()
                        .Except(legalNames)
                        .ToList());

                if (!string.IsNullOrWhiteSpace(name))
                {
                    var newOrgRow = new List<string>();
                    for (var j = 0; j < dataTable.Columns.Count; j++)
                    {
                        newOrgRow.Add(row[j].ToString());
                    }

                    orgs.TryAdd(name, newOrgRow);
                }
            }

            return orgs;
        }

        /// <summary>
        /// Performs exact matching and fuzzy matching between Salesforce, Preqin, and HFM
        /// </summary>
        /// <param name="sfNames">Dictionary with values of each organization's name, and keys of all rows of data associated from Salesforce</param>
        /// <param name="preqinNames">Dictionary with values of each organization's name, and keys of all rows of data associated from Preqin</param>
        /// <param name="hfmNames">Dictionary with values of each organization's name, and keys of all rows of data associated from HFM</param>
        /// <returns>List of 6 lists of data: Preqin exact matches, HFM exact matches, Preqin fuzzy matches, HFM fuzzy matches, all orgs that didn't match, and Salesforce data with a column at the front indicating which list each organization in Salesforce mapped to</returns>
        public static (List<string> preqinMatches, List<string> hfmMatches, List<string> preqinClose, List<string> hfmClose, List<string> noMatch, List<string> matchFile) FindMatches(Dictionary<string, List<string>> sfNames, Dictionary<string, List<string>> preqinNames, Dictionary<string, List<string>> hfmNames)
        {
            var timer = new Stopwatch();
            timer.Start();
            var fuzzyPreqinDict = StringHelperService.ToFuzzy(preqinNames, Constants.PreqinWebsiteColumn, Constants.PreqinEmailColumn);
            var fuzzyHFM = StringHelperService.ToFuzzy(hfmNames, Constants.HfmWebsiteColumn, Constants.HfmEmailColumn);
            var preqinMatches = new List<string>();
            var hfmMatches = new List<string>();
            var preqinClose = new List<string>();
            var hfmClose = new List<string>();
            var noMatch = new List<string>();
            var matchFile = new List<string>();
            timer.Stop();
            Console.WriteLine($"Time taken for getting fuzzy dicts: {timer.Elapsed.TotalMilliseconds}");
            timer.Restart();
            timer.Start();
            foreach (KeyValuePair<string, List<string>> sfOrg in sfNames)
            {
                var sfName = sfOrg.Key;
                var sfWebsiteDomainName = StringHelperService.ParseDomainNameFromWebsite(sfOrg.Value[Constants.SfWebsiteColumn]);
                var sfEmailDomainName1 = StringHelperService.ParseDomainNameFromEmail(sfOrg.Value[Constants.SfEmailColumn1]);
                var sfEmailDomainName2 = StringHelperService.ParseDomainNameFromEmail(sfOrg.Value[Constants.SfEmailColumn2]);
                var entryRow = new StringBuilder();
                var matchRow = new StringBuilder();
                entryRow.AppendFormat($"\"{sfOrg.Value[58]}\",\"{sfOrg.Value[Constants.SfNameColumn]}\",\"{sfOrg.Value[9]}\",\"{sfOrg.Value[10]}\"");
                if (preqinNames.ContainsKey(sfName))
                {
                    foreach (string col in preqinNames[sfName])
                    {
                        entryRow.AppendFormat($",\"{col}\"");
                    }

                    preqinMatches.Add(entryRow.ToString());
                    matchRow.Append("\"Preqin Exact\"");
                }
                else if (hfmNames.ContainsKey(sfName))
                {
                    foreach (string col in hfmNames[sfName])
                    {
                        entryRow.AppendFormat($",\"{col}\"");
                    }

                    hfmMatches.Add(entryRow.ToString());
                    matchRow.Append("\"HFM Exact\"");
                }

                /*            else
                            {
                                int maxWords = 0;
                                string bestMatch = "";
                                foreach(string preqinName in preqinNames.Keys)
                                {
                                    if (WordMatches(SFname, preqinName).Count > maxWords)
                                    {
                                        bestMatch = preqinName;
                                        maxWords = WordMatches(SFname, preqinName).Count;
                                    }
                                }
                                if(maxWords>=(double)SFname.Split(" ").Length / 2 && bestMatch!="")
                                {
                                    foreach (string col in preqinNames[bestMatch])
                                    {
                                        entry = entry + ",\"" + col + "\"";
                                    }
                                    PreqinClose.Add(entry);
                                    matchRow = "\"Preqin Close\"";
                                }
                                else
                                {
                                    maxWords = 0;
                                    bestMatch = "";
                                    foreach (string HFMname in hfmNames.Keys)
                                    {
                                        if (WordMatches(SFname, HFMname).Count > maxWords)
                                        {
                                            bestMatch = HFMname;
                                            maxWords = WordMatches(SFname, HFMname).Count;
                                        }
                                    }
                                    if (maxWords >= (double)SFname.Split(" ").Length / 2 && bestMatch != "")
                                    {
                                        foreach (string col in hfmNames[bestMatch])
                                        {
                                            entry = entry + ",\"" + col + "\"";
                                        }
                                        HFMClose.Add(entry);
                                        matchRow = "\"HFM Close\"";
                                    }
                                    else
                                    {
                                        NoMatch.Add(entry);
                                        matchRow = "\"No Match\"";
                                    }
                                }

                            }
                            foreach (string col in SFOrg.Value)
                            {
                                matchRow = matchRow + ",\"" + col + "\"";
                            }
                            MatchFile.Add(matchRow);*/
                else if (fuzzyPreqinDict.ContainsKey(sfName))
                {
                    foreach (string col in fuzzyPreqinDict[sfName])
                    {
                        entryRow.AppendFormat($",\"{col}\"");
                    }

                    preqinClose.Add(entryRow.ToString());
                    matchRow.Append("\"Preqin Close\"");
                }
                else if (fuzzyPreqinDict.ContainsKey(sfWebsiteDomainName))
                {
                    foreach (string col in fuzzyPreqinDict[sfWebsiteDomainName])
                    {
                        entryRow.AppendFormat($",\"{col}\"");
                    }

                    preqinClose.Add(entryRow.ToString());
                    matchRow.Append("\"Preqin Close\"");
                }
                else if (fuzzyPreqinDict.ContainsKey(sfEmailDomainName1))
                {
                    foreach (string col in fuzzyPreqinDict[sfEmailDomainName1])
                    {
                        entryRow.AppendFormat($",\"{col}\"");
                    }

                    preqinClose.Add(entryRow.ToString());
                    matchRow.Append("\"Preqin Close\"");
                }
                else if (fuzzyPreqinDict.ContainsKey(sfEmailDomainName2))
                {
                    foreach (string col in fuzzyPreqinDict[sfEmailDomainName2])
                    {
                        entryRow.AppendFormat($",\"{col}\"");
                    }

                    preqinClose.Add(entryRow.ToString());
                    matchRow.Append("\"Preqin Close\"");
                }
                else if (fuzzyHFM.ContainsKey(sfName))
                {
                    foreach (string col in fuzzyHFM[sfName])
                    {
                        entryRow.AppendFormat($",\"{col}\"");
                    }

                    hfmClose.Add(entryRow.ToString());
                    matchRow.Append("\"HFM Close\"");
                }
                else if (fuzzyHFM.ContainsKey(sfWebsiteDomainName))
                {// salesforce ID, salesforce orgname, salesforce city, salesforce state, pq/hfm ID, pq/Preqin name, then all pq/Preqin; add logging (total records found in SF, total records matched in pq, total rec match in Preqin, total fuzzy pq, total fuzzy Preqin, total nomatch)
                    foreach (string col in fuzzyHFM[sfWebsiteDomainName])
                    {
                        entryRow.AppendFormat($",\"{col}\"");
                    }

                    hfmClose.Add(entryRow.ToString());
                    matchRow.Append("\"HFM Close\"");
                }
                else if (fuzzyHFM.ContainsKey(sfEmailDomainName1))
                {
                    foreach (string col in fuzzyHFM[sfEmailDomainName1])
                    {
                        entryRow.AppendFormat($",\"{col}\"");
                    }

                    hfmClose.Add(entryRow.ToString());
                    matchRow.Append("\"HFM Close\"");
                }
                else if (fuzzyHFM.ContainsKey(sfEmailDomainName2))
                {
                    foreach (string col in fuzzyHFM[sfEmailDomainName2])
                    {
                        entryRow.AppendFormat($",\"{col}\"");

                        // entry = entry + ",\"" + col + "\"";
                    }

                    hfmClose.Add(entryRow.ToString());
                    matchRow.Append("\"HFM Close\"");
                }
                else
                {
                    noMatch.Add(entryRow.ToString());
                    matchRow.Append("\"No Match\"");
                }

                foreach (string col in sfOrg.Value)
                {
                    // matchRow = matchRow + ",\"" + col + "\"";
                    matchRow.AppendFormat($",\"{col}\"");
                }

                matchFile.Add(matchRow.ToString());
            }

            timer.Stop();
            Console.WriteLine($"Time taken for all string building: {timer.Elapsed.TotalMilliseconds}");
            return (preqinMatches, hfmMatches, preqinClose, hfmClose, noMatch, matchFile);
        }

        /// <summary>
        /// Creates file header based on old file header
        /// </summary>
        /// <param name="file">old file</param>
        /// <param name="headerStart">new columns of file header (appended to start)</param>
        /// <returns>csv format file header</returns>
        public static string MakeHeader(DataTable file, string headerStart)
        {
            var header = headerStart;
            for (var i = 0; i < file.Columns.Count; i++)
            {
                header = header + ",\"" + file.Rows[0][i].ToString() + "\"";
            }

            return header;
        }
    }
}