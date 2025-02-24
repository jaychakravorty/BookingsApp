using Microsoft.AspNetCore.Mvc;
using BookingApp.Core.Domain.Models;
using BookingApp.API.Models;
using BookingApp.Core.Interfaces;

namespace BookingApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IAsyncRepository _repository;

        public BookingController(IAsyncRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("book")]
        public async Task<IActionResult> BookItem([FromQuery] int inventoryId, [FromQuery] int memberId, CancellationToken cancellationToken)
        {
            var member = await _repository.GetAsync<Member>(x => x.Id == memberId, true, cancellationToken).ConfigureAwait(false);

            if (member == null)
                return NotFound($"Member with id {memberId} not found");
            if (member is { BookingCount: >= 2 })
                return BadRequest($"Booking exceeded allowed limit {2}");

            var inventory = await _repository.GetAsync<Inventory>(x => x.Id == inventoryId, true, cancellationToken).ConfigureAwait(false);
            if  (inventory == null)
                return NotFound($"Inventory with {inventoryId} not found");
            if (inventory is { RemainingInventory: <= 0 })
                return BadRequest("Inventory no longer available");

            var booking = new Booking
            {
                MemberId = member.Id,
                InventoryId = inventory.Id,
                BookingDate = DateTime.UtcNow
            };

            //substract from inventory 
            inventory.RemainingInventory = inventory.RemainingInventory <= 0 ? 0 : inventory.RemainingInventory - 1;
            member.BookingCount++;

            await _repository.AddAsync(booking, cancellationToken);

            return Ok(booking);
        }

        [HttpGet]
        public async Task<IActionResult> GetBooking(CancellationToken cancellationToken)
        {
            var bookings = await _repository.GetAllAsNoTrackAsync<Booking>(cancellationToken).ConfigureAwait(false);

            if (bookings is { Count: > 0 })
                return Ok(bookings);
            else
                return NoContent();

        }

        [HttpGet("members")]
        public async Task<IActionResult> GetMembers(CancellationToken cancellationToken)
        {
            var members = await _repository.GetAllAsNoTrackAsync<Member>(cancellationToken).ConfigureAwait(false);


            if (members is { Count: > 0 })
                return Ok(members);
            else
                return NoContent();

        }

        [HttpGet("inventories")]
        public async Task<IActionResult> GetInventories(CancellationToken cancellationToken)
        {
            var inventories = await _repository.GetAllAsNoTrackAsync<Inventory>(cancellationToken).ConfigureAwait(false);


            if (inventories is { Count: > 0 })
                return Ok(inventories);
            else
                return NoContent();

        }

        [HttpPost("cancel")]
        public async Task<IActionResult> CancelBooking([FromQuery] int bookingId, CancellationToken cancellationToken)
        {
            var booking = await this._repository.GetAsync<Booking>(b => b.Id == bookingId, true, cancellationToken);
            if (booking == null)
                return NotFound("Booking not found");
            booking.Inventory.RemainingInventory++;

            //substract from booking count if greater than 0
            booking.Member.BookingCount = booking.Member.BookingCount <= 0 ? 0 : booking.Member.BookingCount--;

            await _repository.DeleteAsync(booking, cancellationToken);

            return Ok();
        }
    }
}
