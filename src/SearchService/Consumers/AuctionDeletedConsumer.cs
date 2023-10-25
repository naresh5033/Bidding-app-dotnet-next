using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService;

public class AuctionDeletedConsumer : IConsumer<AuctionDeleted> //here we don't need to Di into ctor
{
    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        Console.WriteLine("--> Consuming AuctionDeleted: " + context.Message.Id);

        var result = await DB.DeleteAsync<Item>(context.Message.Id); //msg.id aka auction id

        if (!result.IsAcknowledged) 
            throw new MessageException(typeof(AuctionDeleted), "Problem deleting auction");
    }
}
