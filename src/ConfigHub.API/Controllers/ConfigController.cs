using ConfigHub.Domain.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("component/{component}/key/{key}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetConfigByKey(string component, string key)
        {
            try
            {
                var applicationId = this.GetApplicationId();
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
        public async Task<IActionResult> GetAllConfigsByComponent(string component)
        {
            try
            {
                var applicationId = this.GetApplicationId();
                var configs = await _configService.GetAllConfigItemsByComponent(applicationId, component);
                if (configs != null && configs.Count() > 0)
                {
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

        private string GetApplicationId()
        {
            // Assuming the application ID is provided as a header named "X-ApplicationId"
            return Request.Headers["X-ApplicationId"].ToString();
        }
    }
}
