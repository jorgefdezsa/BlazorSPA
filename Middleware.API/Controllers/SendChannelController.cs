namespace Middleware.API.Controllers
{
    using Azure.Messaging.ServiceBus;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Middleware.API.Models;

    [ApiController]
    [Route("api/[controller]")]
    public class SendChannelController : ControllerBase
    {
        private readonly ServiceBusOptions _options;
        private readonly ILogger<SendChannelController> _logger;

        public SendChannelController(
            IOptions<ServiceBusOptions> options,
            ILogger<SendChannelController> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        [HttpPost]
        [Authorize] // 🔐 Requiere token JWT válido
        public async Task<IActionResult> Post([FromBody] MessageDto dto)
        {
            var username = User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

            if (string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("Claim 'preferred_username' no encontrada.");
                return Unauthorized("Claim 'preferred_username' no encontrada.");
            }

            if (string.IsNullOrWhiteSpace(dto?.Msg))
            {
                _logger.LogWarning("El cuerpo del mensaje está vacío.");
                return BadRequest("Se requiere un campo 'msg' con contenido.");
            }

            _logger.LogInformation("Token válido para usuario: {Usuario}", username);

            await using var client = new ServiceBusClient(_options.ConnectionString);
            var sender = client.CreateSender(_options.QueueName);

            var payload = $"[{DateTime.UtcNow:O}] {username}: {dto.Msg}";
            var message = new ServiceBusMessage(payload);

            await sender.SendMessageAsync(message);

            return Ok("Mensaje enviado correctamente.");
        }
    }
}
