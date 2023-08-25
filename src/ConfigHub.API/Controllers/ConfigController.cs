using ConfigHub.API.CustomAttribute;
using ConfigHub.Domain.Entity;
using ConfigHub.Domain.Interface;
using ConfigHub.Infrastructure.Services;
using ConfigHub.Shared;
using ConfigHub.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using ConfigHub.Shared.Entity.ConfigHub.Domain.Entity;
using ConfigHub.Shared.Entity;
using Microsoft.AspNetCore.Builder;

namespace ConfigHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly IConfigService _configService;
        private readonly ILogger<ConfigController> _logger;

        public ConfigController(IConfigService configService, ILogger<ConfigController> logger)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ConfigItem), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddConfigItem([FromBody] ConfigItem configItem)
        {
            try
            {
                if (configItem == null)
                {
                    return BadRequest("Invalid data");
                }

                await _configService.AddConfigItemAsync(configItem);

                return CreatedAtAction(nameof(GetConfigByKey), new { component = configItem.Component, key = configItem.Key }, configItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{component}/{key}")]
        [ProducesResponseType(typeof(ConfigItem), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
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

                var updatedConfigItem = await _configService.UpdateConfigItemAsync(configItem);

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
        [ProducesResponseType(typeof(IEnumerable<AppInfo>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAllAppInfoAsync()
        {
            try
            {
                var appInfos = await _configService.GetAllAppInfoAsync();

                if (appInfos != null && appInfos.Any())
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
        [ProducesResponseType(typeof(ConfigItem), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetConfigByKey(string component, string key)
        {
            try
            {
                var applicationId = GetApplicationName();
                var config = await _configService.GetConfigItemByKeyAndComponent(applicationId, component, key);
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
        [Paging]
        [ProducesResponseType(typeof(IEnumerable<ConfigItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAllConfigsByComponent(string component, [FromQuery] bool includeValue = true)
        {
            try
            {
                var applicationId = GetApplicationName();
                var (configs, totalCount) = await _configService.GetAllConfigItemsByComponent(applicationId, component, this.Request.GetTake(), this.Request.GetSkip());

                if (configs != null && configs.Any())
                {
                    if (!includeValue)
                    {
                        foreach (var item in configs)
                        {
                            item.Value = null;
                        }
                    }

                    this.Response.Headers.Add(Constants.TotalCountResponseHeader, totalCount.ToString());
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
        [Paging]
        [ProducesResponseType(typeof(IEnumerable<ConfigItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchConfigs([FromQuery] string search = "")
        {
            if (string.IsNullOrEmpty(search) || search.Length < Constants.MinimumSearchLength)
            {
                return BadRequest($"Search query must be at least {Constants.MinimumSearchLength} characters long.");
            }

            try
            {
                var (configItems, totalCount) = await _configService.SearchConfigItems(search, this.Request.GetTake(), this.Request.GetSkip());
                this.Response.Headers.Add(Constants.TotalCountResponseHeader, totalCount.ToString());
                return Ok(configItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching configurations.");
                return StatusCode(500, "An error occurred while fetching configurations.");
            }
        }

        [HttpDelete("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteConfigItem(string id)
        {
            try
            {
                var configItem = await _configService.GetConfigItemByIdAsync(id);

                if (configItem == null)
                {
                    return NotFound("Config item not found");
                }

                await _configService.DeleteConfigItemAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}/history")]
        [AllowAnonymous]
        [Paging]
        [ProducesResponseType(typeof(IEnumerable<ConfigItemHistory>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetConfigItemHistoryById(string id)
        {
            try
            {
                var (historyItems, totalCount) = await _configService.GetConfigItemHistoryByIdAsync(id, this.Request.GetTake(), this.Request.GetSkip());
                this.Response.Headers.Add(Constants.TotalCountResponseHeader, totalCount.ToString());
                return Ok(historyItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching config item history by ID.");
                return StatusCode(500, "An error occurred while fetching config item history by ID.");
            }
        }

        [HttpGet("component/{component}/history")]
        [AllowAnonymous]
        [Paging]
        [ProducesResponseType(typeof(IEnumerable<ConfigItemHistory>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetConfigItemHistoryByComponent(string component, [FromQuery] OperationType? operationType = null)
        {
            try
            {
                var applicationId = this.GetApplicationName();
                var (historyItems, totalCount) = await _configService.GetConfigItemHistoryByComponentAsync(applicationId, component, operationType, this.Request.GetTake(), this.Request.GetSkip());

                this.Response.Headers.Add(Constants.TotalCountResponseHeader, totalCount.ToString());
                return Ok(historyItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching config item history by operation.");
                return StatusCode(500, "An error occurred while fetching config item history by operation.");
            }
        }

        [HttpPost("app")]
        [ProducesResponseType(typeof(AppInfo), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddApplication([FromBody] AppInfo appInfo)
        {
            try
            {
                var addedApp = await _configService.AddApplicationAsync(appInfo);
                return Created("", new { Message = "Application added successfully", AppName = addedApp.ApplicationName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{appId}/component")]
        [ProducesResponseType(typeof(ComponentInfo), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddComponent(string appId, [FromBody] ComponentInfo component)
        {
            try
            {
                var addedComponent = await _configService.AddComponentAsync(appId, component);
                return Created("", new { Message = "Component added successfully", ComponentName = addedComponent.Name });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{appId}/component/clone")]
        [ProducesResponseType(typeof(ComponentInfo), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CloneComponent(string appId, [FromBody] CloneComponentRequest request)
        {
            try
            {
                var clonedComponent = await _configService.CloneComponentAsync(appId, request);
                return Created("", new { Message = "Component cloned successfully", ComponentName = clonedComponent.Name });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("application/{applicationName}/component/{componentName}")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteComponent(string applicationName, string componentName)
        {
            try
            {
                var deleted = await _configService.DeleteComponentAsync(applicationName, componentName);

                if (deleted)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound("Component not found");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        private string GetApplicationName()
        {
            return Request.Headers[Constants.ApplicationNameHeader].ToString();
        }
    }
}
