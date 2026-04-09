using DirectoryService.Contracts.Locations.CreateLocations;

namespace DirectoryService.Contracts.Locations.UpdateLocations;

public record UpdateLocationRequest(string Name, AddressDto Address, string Timezone);