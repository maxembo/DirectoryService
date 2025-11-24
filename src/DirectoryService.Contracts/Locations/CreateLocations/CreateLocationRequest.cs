namespace DirectoryService.Contracts.Locations.CreateLocations;

public record CreateLocationRequest(string Name, AddressDto Address, string Timezone);