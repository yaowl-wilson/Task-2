using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Net.Http;

using OrderService.Models;
using OrderService.Contexts;

namespace OrderService.Controllers
{
    public class OrderController : Controller
    {
        public OrderModelContext _context;

        public OrderController(
            OrderModelContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderModel>>> GetOrderItems()
        {
            var orderItems = await _context.OrderModelItems.ToListAsync();
            return orderItems;
        }
        
    }
}
