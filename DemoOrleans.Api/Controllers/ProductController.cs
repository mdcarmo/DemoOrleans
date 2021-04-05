using DemoOrleans.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using System;
using System.Threading.Tasks;

namespace DemoOrleans.Api.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IClusterClient _client;

        public ProductController(IClusterClient client)
        {
            _client = client;
        }

        // GET api/products/5
        [HttpGet("{productId}")]
        public async Task<IActionResult> Get(int productId)
        {
            try
            {
                var product = _client.GetGrain<IProduct>(productId);
                int stockQuantity = await product.GetStock();
                return Ok(stockQuantity);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // PUT api/products/5
        [HttpPut("{productId}")]
        public async Task<IActionResult> Put(int productId, [FromForm] string value)
        {
            try
            {
                var product = _client.GetGrain<IProduct>(productId);
                await product.SetStock(int.Parse(value));
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
