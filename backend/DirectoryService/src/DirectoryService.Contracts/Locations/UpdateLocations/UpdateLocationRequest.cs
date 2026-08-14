namespace DirectoryService.Contracts.Locations.UpdateLocations;

public record UpdateLocationRequest(string Name, AddressRequest Address, string Timezone);