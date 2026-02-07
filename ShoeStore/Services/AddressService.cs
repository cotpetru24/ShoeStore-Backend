using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Address;

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
                Country = request.Country,
                IsActive = true
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
                .Where(a => a.UserId == userId && a.IsActive == true)
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


        public async Task<CreateAddressResponseDto> UpdateAddressAsync(AddressDto request, string userId)
        {
            var address = await _context.UserAddresses
                .Where(a => a.Id == request.Id && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (address == null)
                throw new ArgumentException("Address not found");

            address.AddressLine1 = request.AddressLine1;
            address.City = request.City;
            address.Postcode = request.Postcode;
            address.Country = request.Country;

            await _context.SaveChangesAsync();

            return new CreateAddressResponseDto
            {
                Id = address.Id,
                Message = "Address updated successfully",
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

            address.IsActive = false;

            await _context.SaveChangesAsync();

            return true;
        }

    }
}
