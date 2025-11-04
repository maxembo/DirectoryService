namespace DirectoryService.Contracts.Locations;

public record AddressDto(
    string City,
    string Country,
    string Street,
    string House);