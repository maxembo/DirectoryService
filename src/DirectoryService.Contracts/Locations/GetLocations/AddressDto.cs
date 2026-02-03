using Shared.Dapper;

namespace DirectoryService.Contracts.Locations.GetLocations;

public record AddressDto(string City, string Country, string Street, string House) : IDapperJson;
