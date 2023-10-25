namespace AuctionService.IntegrationTests;

[CollectionDefinition("Shared collection")] //we can use this collection definition name in our test classes
public class SharedFixture : ICollectionFixture<CustomWebAppFactory> //so our test classes can share web instance and single db rather than creating individual
{

}
