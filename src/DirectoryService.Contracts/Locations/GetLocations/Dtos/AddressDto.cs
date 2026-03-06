using Shared.Core.Dapper;

namespace DirectoryService.Contracts.Locations.GetLocations.Dtos;

public record AddressDto(string City, string Country, string Street, string House) : IDapperJson;
