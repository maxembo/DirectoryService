namespace DirectoryService.Contracts.Locations.CreateLocations;

public record CreateLocationRequest(string Name, AddressRequest Address, string Timezone);