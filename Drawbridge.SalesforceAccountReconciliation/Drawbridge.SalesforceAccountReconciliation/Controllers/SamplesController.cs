// -----------------------------------------------------------------------
// <copyright file="SamplesController.cs" company="Drawbridge Partners, LLC">
// Copyright (c) Drawbridge Partners, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Drawbridge.SalesforceAccountReconciliation.Configuration;
using Drawbridge.SalesforceAccountReconciliation.Models;
using Drawbridge.WebApi.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Drawbridge.SalesforceAccountReconciliation.Controllers
{
    /// <summary>
    /// Sample controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class SamplesController : ControllerBase
    {
        private static List<Sample> _samples;
        private readonly SampleConfiguration _sampleConfiguration;

        private readonly ILogger<SamplesController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SamplesController"/> class.
        /// </summary>
        /// <param name="sampleConfiguration">The sample configuration.</param>
        /// <param name="logger">The logger.</param>
        public SamplesController(
            IOptions<SampleConfiguration> sampleConfiguration,
            ILogger<SamplesController> logger)
        {
            if (sampleConfiguration is null)
            {
                throw new ArgumentNullException(nameof(sampleConfiguration));
            }

            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _samples = new List<Sample>
            {
                new Sample { Id = 1, Name = "foo", Color = "green" },
                new Sample { Id = 2, Name = "bar", Color = "red" }
            };

            _sampleConfiguration = sampleConfiguration.Value;
            _logger = logger;
        }

        /// <summary>
        /// Gets all samples.
        /// </summary>
        /// <returns>An instance of <see cref="ActionResult" />.</returns>
        [HttpGet]
        [Route("")]
        [ProducesResponseType(typeof(IEnumerable<Sample>), 200)]
        public ActionResult GetAll()
        {
            _logger.LogInformation("Hit get endpoint");

            return Ok(_samples);
        }

        /// <summary>
        /// Gets one sample.
        /// </summary>
        /// <param name="id">The sample id.</param>
        /// <returns>An instance of <see cref="ActionResult" />.</returns>
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(Sample), 200)]
        [ProducesResponseType(404)]
        public ActionResult Get(int id)
        {
            var sample = _samples.FirstOrDefault(x => x.Id == id);

            return sample is null ? NotFound() : Ok(sample);
        }

        /// <summary>
        /// Adds a sample.
        /// </summary>
        /// <param name="request">The add sample request.</param>
        /// <returns>An instance of <see cref="ActionResult" />.</returns>
        [HttpPost]
        [Route("")]
        [JsonSchemaValidationFilter("addSample.json")]
        [ProducesResponseType(typeof(Dictionary<string, string[]>), 400)]
        [ProducesResponseType(typeof(Sample), 201)]
        [ProducesResponseType(500)]
        public ActionResult Add(AddSampleRequest request)
        {
            if (_samples.Count == _sampleConfiguration.MaxAllowed)
            {
                throw new Exception("Max samples has been reached");
            }

            var newSample = new Sample { Id = _samples.Count + 1, Name = request.Name, Color = request.Color };
            _samples.Add(newSample);

            return new CreatedAtActionResult(nameof(Get), "Tasks", new { id = newSample.Id }, newSample);
        }
    }
}
