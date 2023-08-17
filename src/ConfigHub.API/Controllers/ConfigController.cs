using ConfigHub.Domain.Entity;
using ConfigHub.Domain.Interface;
using ConfigHub.Shared;
using ConfigHub.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

namespace ConfigHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly IConfigService configService;
        private readonly ILogger<ConfigController> _logger;

        public ConfigController(IConfigService configService, ILogger<ConfigController> logger)
        {
            this.configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        public async Task<IActionResult> AddConfigItem([FromBody] ConfigItem configItem)
        {
            try
            {
                if (configItem == null)
                {
                    return BadRequest("Invalid data");
                }

                // Add the config item to the database
                await this.configService.AddConfigItemAsync(configItem);

                return CreatedAtAction(nameof(GetConfigByKey), new { component = configItem.Component, key = configItem.Key }, configItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{component}/{key}")]
        public async Task<IActionResult> UpdateConfigItem(string component, string key, [FromBody] ConfigItem configItem)
        {
            try
            {
                if (configItem == null)
                {
                    return BadRequest("Invalid data");
                }

                configItem.Component = component;
                configItem.Key = key;

                // Update the config item in the database
                var updatedConfigItem = await this.configService.UpdateConfigItemAsync(configItem);

                return Ok(updatedConfigItem);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpGet("appinfo")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllAppInfoAsync()
        {
            try
            {
                var appInfos = await this.configService.GetAllAppInfoAsync();

                if (appInfos != null && appInfos.Count() > 0)
                {
                    return Ok(appInfos);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching application information.");
                return StatusCode(500, "An error occurred while fetching application information.");
            }
        }


        [HttpGet("component/{component}/key/{key}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetConfigByKey(string component, string key)
        {
            try
            {
                var applicationId = this.GetApplicationName();
                var config = await this.configService.GetConfigItemByKeyAndComponent(applicationId, component, key);
                if (config != null)
                {
                    return Ok(config);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching configuration by key.");
                return StatusCode(500, "An error occurred while fetching configuration by key.");
            }
        }

        [HttpGet("component/{component}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllConfigsByComponent(string component, [FromQuery] bool includeValue = true)
        {
            try
            {
                var applicationId = this.GetApplicationName();
                var configs = await this.configService.GetAllConfigItemsByComponent(applicationId, component);

                if (configs != null && configs.Count() > 0)
                {
                    if (!includeValue)
                    {
                        foreach (var item in configs)
                        {
                            item.Value = null;
                        }
                    }

                    return Ok(configs);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching configurations by component.");
                return StatusCode(500, "An error occurred while fetching configurations by component.");
            }
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchConfigs([FromQuery] int? take = 100, [FromQuery] int? skip = 0, [FromQuery] string search = "")
        {
            if (take == null || take <= 0)
            {
                take = Constants.DefaultPagingSize; // Set default value if take is not provided or negative
            }

            if (skip == null || skip < 0)
            {
                skip = 0; // Set default value if skip is not provided or negative
            }

            if (string.IsNullOrEmpty(search) || search.Length < Constants.MinimumSearchLength)
            {
                return BadRequest($"Search query must be at least {Constants.MinimumSearchLength} characters long.");
            }

            try
            {
                var configItems = await this.configService.SearchConfigItems(search, take.Value, skip.Value);

                return Ok(configItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching configurations.");
                return StatusCode(500, "An error occurred while fetching configurations.");
            }
        }

        private string GetApplicationName()
        {
            return Request.Headers[Constants.ApplicationNameHeader].ToString();
        }
    }
}
