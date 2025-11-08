namespace DirectoryService.Contracts.Locations.CreateLocations;

public record AddressDto(
    string City,
    string Country,
    string Street,
    string House);