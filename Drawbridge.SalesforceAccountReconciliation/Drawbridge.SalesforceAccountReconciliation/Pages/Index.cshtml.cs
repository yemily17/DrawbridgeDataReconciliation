// -----------------------------------------------------------------------
// <copyright file="Index.cshtml.cs" company="Drawbridge Partners, LLC">
// Copyright (c) Drawbridge Partners, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Drawbridge.SalesforceAccountReconciliation;
using Drawbridge.SalesforceAccountReconciliation.Services;
using ExcelDataReader;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Drawbridge.SalesforceAccountReconciliation.Pages
{
    /// <summary>
    /// Main page of web app
    /// </summary>
    public class Index : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="Index"/> class.
        /// </summary>
        /// <param name="webHostEnvironment">IWebHostEnvironment to use</param>
        public Index(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Gets or sets first file inputted from user
        /// </summary>
        [BindProperty]
        public IFormFile SfFile { get; set; }

        /// <summary>
        /// Gets or sets second file inputted from user
        /// </summary>
        [BindProperty]
        public IFormFile PreqinFile { get; set; }

        /// <summary>
        /// Gets or sets third file inputted from user
        /// </summary>
        [BindProperty]
        public IFormFile HfmFile { get; set; }

        /// <summary>
        /// Gets or sets comparison result
        /// </summary>
        public string ComparisonResult { get; set; }

        /// <summary>
        /// Runs on get
        /// </summary>
        public void OnGet()
        {
        }

        /// <summary>
        /// Runs when compare button clicked on UI
        /// </summary>
        /// <returns>zip folder containing 6 result csv files</returns>
        public ActionResult OnPost()
        {
            var timer = new Stopwatch();
            timer.Start();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var sfNames = ReconciliationReportHelperService.BuildDataTable(SfFile);
            var sfOrgs = ReconciliationReportHelperService.ToDictionary(sfNames, Constants.SfNameColumn);
            timer.Stop();
            Console.WriteLine("SF time " + timer.ElapsedMilliseconds);
            timer.Reset();
            timer.Start();
            var preqinNames = ReconciliationReportHelperService.BuildDataTable(PreqinFile);
            var preqinOrgs = ReconciliationReportHelperService.ToDictionary(preqinNames, Constants.PreqinNameColumn);
            Console.WriteLine("DATA " + preqinNames.Rows[2][32]);
            timer.Stop();
            Console.WriteLine("Preqin time " + timer.ElapsedMilliseconds);
            timer.Reset();
            timer.Start();
            var hfmNames = ReconciliationReportHelperService.BuildDataTable(HfmFile);
            var hfmOrgs = ReconciliationReportHelperService.ToDictionary(hfmNames, Constants.HfmNameColumn);
            timer.Stop();
            Console.WriteLine("HFM time " + timer.ElapsedMilliseconds);
            timer.Reset();
            timer.Start();
            var outputFiles = ReconciliationReportHelperService.FindMatches(sfOrgs, preqinOrgs, hfmOrgs);
            timer.Stop();
            Console.WriteLine("Matches time " + timer.ElapsedMilliseconds);
            timer.Reset();
            var preqinHeader = ReconciliationReportHelperService.MakeHeader(preqinNames, Constants.SfStandardHeader);
            var hfmHeader = ReconciliationReportHelperService.MakeHeader(hfmNames, Constants.SfStandardHeader);
            var sfHeader = Constants.SfStandardHeader;
            var sfResHeader = ReconciliationReportHelperService.MakeHeader(sfNames, "\"Match File\"");

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var preqinMatchFile = archive.CreateEntry(Constants.PreqinMatchFileName);
                    using (var streamWriter = new StreamWriter(preqinMatchFile.Open()))
                    {
                        streamWriter.Write(preqinHeader + "\n" + string.Join("\n", outputFiles.preqinMatches.ToArray())); // build csv here header and content
                    }

                    var hfmMatchFile = archive.CreateEntry(Constants.HfmMatchFileName);
                    using (var streamWriter = new StreamWriter(hfmMatchFile.Open()))
                    {
                        streamWriter.Write(hfmHeader + "\n" + string.Join("\n", outputFiles.hfmMatches.ToArray()));
                    }

                    var preqinCloseFile = archive.CreateEntry(Constants.PreqinCloseFileName);
                    using (var streamWriter = new StreamWriter(preqinCloseFile.Open()))
                    {
                        streamWriter.Write(preqinHeader + "\n" + string.Join("\n", outputFiles.preqinClose.ToArray()));
                    }

                    var hfmCloseFile = archive.CreateEntry(Constants.HfmCloseFileName);
                    using (var streamWriter = new StreamWriter(hfmCloseFile.Open()))
                    {
                        streamWriter.Write(hfmHeader + "\n" + string.Join("\n", outputFiles.hfmClose.ToArray()));
                    }

                    var noMatchFile = archive.CreateEntry(Constants.NoMatchFileName);
                    using (var streamWriter = new StreamWriter(noMatchFile.Open()))
                    {
                        streamWriter.Write(sfHeader + "\n" + string.Join("\n", outputFiles.noMatch.ToArray()));
                    }

                    var sfResultsFile = archive.CreateEntry(Constants.SfResultsFileName);
                    using (var streamWriter = new StreamWriter(sfResultsFile.Open()))
                    {
                        streamWriter.Write(sfResHeader + "\n" + string.Join("\n", outputFiles.matchFile.ToArray()));
                    }
                }

                timer.Stop();
                Console.WriteLine("end part " + timer.ElapsedMilliseconds);
                return File(memoryStream.ToArray(), "application/zip", "results" + DateTime.Now.ToString("MM-dd-yyyy") + ".zip");
            }
        }
    }
}