using AuctionService.DTOs;
using AuctionService.Entities;

namespace AuctionService;

public interface IAuctionRepository
{
    void AddAuction(Auction auction);

    Task<AuctionDto> GetAuctionByIdAsync(Guid id);

    Task<Auction> GetAuctionEntityById(Guid id);
    Task<List<AuctionDto>> GetAuctionsAsync(string date);
    void RemoveAuction(Auction auction);
    Task<bool> SaveChangesAsync();
}
