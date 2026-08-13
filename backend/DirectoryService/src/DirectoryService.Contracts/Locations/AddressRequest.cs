namespace DirectoryService.Contracts.Locations;

public record AddressRequest(
    string City,
    string Country,
    string Street,
    string House);