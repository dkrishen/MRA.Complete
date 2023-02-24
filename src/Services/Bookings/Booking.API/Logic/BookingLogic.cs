﻿using MRA.Bookings.Models;
using MRA.Bookings.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MRA.Bookings.Logic
{
    public class BookingLogic : IBookingLogic
    {
        private readonly IBookingRepository _bookingRepository;

        public BookingLogic(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
        {
            return await _bookingRepository.GetBookingByIdAsync(bookingId);
        }

        public async Task<bool> AddBookingAsync(Booking booking)
        {
            if (await ValidateBooking(booking))
            {
                return await _bookingRepository.AddBookingAsync(booking);
            }

            return false;
        }

        public async Task<bool> UpdateBookingAsync(Booking booking)
        {
            if (await ValidateBooking(booking))
            {
                return await _bookingRepository.UpdateBookingAsync(booking);
            }

            return false;
        }

        public async Task<bool> DeleteBookingAsync(Guid id)
        {
            return await _bookingRepository.DeleteBookingAsync(id);
        }

        public async Task<IEnumerable<Booking>> GetBookingsAsync()
        {
            return await _bookingRepository.GetBookingsAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(Guid userId)
        {
            return await _bookingRepository.GetBookingsByUserIdAsync(userId);
        }

        private async Task<bool> ValidateBooking(Booking booking)
        {
            var allBookings = await _bookingRepository.GetBookingsAsync();
            var collisions = FindCollisions(allBookings, booking).ToList();
            var currentBooking = await _bookingRepository.GetBookingByIdAsync(booking.Id);

            if(currentBooking != null)
            {
                collisions.Remove(currentBooking);
            }

            if (collisions?.Count() != 0)
            {
                return false;
            }

            return true;
        }

        private IEnumerable<Booking> FindCollisions(IEnumerable<Booking> allBookings, Booking booking)
        {
            return allBookings
                .Where(b => b.Date == booking.Date)
                .Where(b => CheckCollision(b, booking));
        }

        private bool CheckCollision(Booking first, Booking second)
        {
            if ((first.StartTime >= second.StartTime && first.StartTime < second.EndTime) ||
              (first.EndTime <= second.EndTime && first.EndTime > second.StartTime) ||
              (second.StartTime > first.StartTime && second.EndTime < first.EndTime))
            {
                return true;
            }
            else return false;
        }
    }
}