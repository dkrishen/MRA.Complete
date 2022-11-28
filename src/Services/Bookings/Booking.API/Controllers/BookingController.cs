﻿using MRA.Bookings.Models;
using MRA.Bookings.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using MRA.Bookings.Logic;

namespace MRA.Bookings.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly ILogger<BookingController> _logger;
        private readonly IBookingLogic _bookingLogic;

        public BookingController(ILogger<BookingController> logger, IBookingLogic bookingLogic)
        {
            _logger = logger;
            _bookingLogic = bookingLogic;
        }

        [HttpGet]
        [Route("GetAllBookings")]
        public async Task<IActionResult> GetBookingsAsync()
        {
            var bookings = await _bookingLogic.GetBookingsAsync();
            return Ok(bookings);
        }

        [HttpGet]
        [Route("GetBookingsByUserId")]
        public async Task<IActionResult> GetBookingsByUserIdAsync(string data)
        {
            Guid userId = JsonConvert.DeserializeObject<Guid>(data);
            return Ok(await _bookingLogic.GetBookingsByUserIdAsync(userId));
        }

        [HttpPost]
        [Route("AddBooking")]
        public async Task<IActionResult> AddBookingAsync([FromBody] Booking data)
        {
            await _bookingLogic.AddBookingAsync(data);
            return Ok(true);
        }

        [HttpDelete]
        [Route("deleteBooking")]
        public async Task<IActionResult> DeleteBookingAsync([FromBody] Guid data)
        {
           await _bookingLogic.DeleteBookingAsync(data);
            return Ok(true);
        }

        [HttpPut]
        [Route("updateBooking")]
        public async Task<IActionResult> UpdateBookingAsync([FromBody] Booking data)
        {
            await _bookingLogic.UpdateBookingAsync(data);
            return Ok(true);
        }
    }
}
