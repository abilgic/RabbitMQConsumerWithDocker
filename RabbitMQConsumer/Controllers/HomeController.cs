using Microsoft.AspNetCore.Mvc;
using RabbitMQConsumer.Models;
using System.Diagnostics;

namespace RabbitMQConsumer.Controllers
{
    public class HomeController : Controller
    {
        private readonly RabbitMqConsumerService _rabbitMqConsumerService;

        public HomeController(RabbitMqConsumerService rabbitMqConsumerService)
        {
            _rabbitMqConsumerService = rabbitMqConsumerService;
        }
        public IActionResult Index()
        {
            var message = _rabbitMqConsumerService.GetMessage();
            if (string.IsNullOrEmpty(message))
            {
                return NotFound("No new messages");
            }

            return Ok(message);

        }
    }

}
