namespace DirectoryService.Contracts.Locations.CreateLocations;

public record CreateLocationDto(
    string Name,
    string Timezone,
    AddressDto Address);