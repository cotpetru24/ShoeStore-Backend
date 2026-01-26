using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Order;

namespace ShoeStore.Services
{
    public class AddressService
    {
        private readonly ShoeStoreContext _context;

        public AddressService(ShoeStoreContext context)
        {
            _context = context;
        }


        public async Task<CreateAddressResponseDto> CreateAddressAsync(CreateAddressRequestDto request, string userId)
        {
            var newAddress = new UserAddress
            {
                UserId = userId,
                AddressLine1 = request.AddressLine1,
                City = request.City,
                Postcode = request.Postcode,
                Country = request.Country
            };

            _context.UserAddresses.Add(newAddress);
            await _context.SaveChangesAsync();

            return new CreateAddressResponseDto
            {
                Id = newAddress.Id,
                Message = "Shipping address created successfully",
                CreatedAt = DateTime.UtcNow
            };
        }


        public async Task<AddressDto?> GetAddressByIdAsync(int addressId, string userId)
        {
            var address = await _context.UserAddresses
                .Where(a => a.Id == addressId && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (address == null)
                return null;

            return new AddressDto
            {
                Id = address.Id,
                UserId = address.UserId,
                AddressLine1 = address.AddressLine1,
                City = address.City,
                Postcode = address.Postcode,
                Country = address.Country
            };
        }


        public async Task<List<AddressDto>> GetUserAddressesAsync(string userId)
        {
            var addresses = await _context.UserAddresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            return addresses.Select(a => new AddressDto
            {
                Id = a.Id,
                UserId = a.UserId,
                AddressLine1 = a.AddressLine1,
                City = a.City,
                Postcode = a.Postcode,
                Country = a.Country
            }).ToList();
        }


        public async Task<CreateAddressResponseDto> UpdateAddressAsync( AddressDto request, string userId)
        {
            var address = await _context.UserAddresses
                .Where(a => a.Id == request.Id && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (address == null)
                throw new ArgumentException("Shipping address not found");

            address.AddressLine1 = request.AddressLine1;
            address.City = request.City;
            address.Postcode = request.Postcode;
            address.Country = request.Country;

            await _context.SaveChangesAsync();

            return new CreateAddressResponseDto
            {
                Id = address.Id,
                Message = "Shipping address updated successfully",
                CreatedAt = DateTime.UtcNow
            };
        }


        public async Task<bool> DeleteAddressAsync(int addressId, string userId)
        {
            var address = await _context.UserAddresses
                .Where(a => a.Id == addressId && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (address == null)
                return false;

            // Check if address is being used in any orders
            var isUsedInOrders = await _context.Orders
                .AnyAsync(o => o.ShippingAddressId == addressId || o.BillingAddressId == addressId);

            if (isUsedInOrders)
                throw new InvalidOperationException("Cannot delete address that is being used in orders");

            _context.UserAddresses.Remove(address);
            await _context.SaveChangesAsync();

            return true;
        }

    }
}
