using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Drawbridge.SalesforceAccountReconciliation.Pages;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using Azure.Core;
using NSubstitute;
using Azure;
using Namotion.Reflection;
using System.IO.Compression;

namespace Drawbridge.SalesforceAccountReconciliation.Tests
{
    public class FileTest
    {
        [Fact]
        public void OnPostSuccesful()
        {
            // Arrange
            var sfFile = Substitute.For<IFormFile>();
            var sfFileStream = File.Open("testSfFile.xlsx", FileMode.Open, FileAccess.Read);
            sfFile.OpenReadStream().Returns(sfFileStream);

            var preqinFile = Substitute.For<IFormFile>();
            var preqinFileStream = File.Open("testPreqinFile.xlsx", FileMode.Open, FileAccess.Read);
            preqinFile.OpenReadStream().Returns(preqinFileStream);

            var hfmFile = Substitute.For<IFormFile>();
            var hfmFileStream = File.Open("testHfmFile.xlsx", FileMode.Open, FileAccess.Read);
            hfmFile.OpenReadStream().Returns(hfmFileStream);

            var mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
            var pageModel = new Pages.Index(mockWebHostEnvironment.Object) { SfFile = sfFile, PreqinFile = preqinFile, HfmFile = hfmFile };

            // Act
            var response = pageModel.OnPost();

            // Assert
            response.Should().BeOfType<FileContentResult>();
            response.HasProperty("ContentType").Should().BeTrue(); //filecontents
            response.GetType().GetProperty("ContentType").GetValue(response, null).Should().Be("application/zip");
            var responseFileBytes = (byte[])response.GetType().GetProperty("FileContents").GetValue(response, null); // application/zip
            Stream stream = new MemoryStream(responseFileBytes);
            var zip = new ZipArchive(stream);
            zip.Entries.Count.Should().Be(6);
            string[] testFileNames = { "testPreqinMatches.csv", "testHFMMatches.csv", "testPreqinClose.csv", "testHFMClose.csv", "testNoMatch.csv", "testSFResults.csv" };
            String fileContents;
            String correctFileContents;
            for (int i = 0; i < zip.Entries.Count; i++)
            {
                var file = zip.Entries[i].Open();
                using (var reader = new StreamReader(file))
                {
                    fileContents = reader.ReadToEnd();
                }
                using (var checkStream = File.OpenRead(testFileNames[i]))
                using (var reader = new StreamReader(checkStream))
                {
                    correctFileContents = reader.ReadToEnd();
                }
                Console.WriteLine("WWWWWWWWWWWW");
                Console.WriteLine(correctFileContents);
                Console.WriteLine(fileContents);
                fileContents.Should().Be(correctFileContents);
            }
        }
    }
}
