using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Location;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using Shared;
using Address = DirectoryService.Domain.Locations.ValueObjects.Address;

namespace DirectoryService.Application;

public class CreateLocationHandle
{
    private readonly ILocationsRepository _locationsRepository;

    public CreateLocationHandle(ILocationsRepository locationsRepository)
    {
        _locationsRepository = locationsRepository;
    }

    public async Task<Result<Guid, Error>> Handle(
        CreateLocationRequest locationRequest,
        CancellationToken cancellationToken)
    {
        LocationId locationId = LocationId.NewLocationId();

        var locationNameResult = LocationName.Create(locationRequest.Name);
        if (locationNameResult.IsFailure)
        {
            return locationNameResult.Error;
        }

        LocationName locationName = locationNameResult.Value;

        var locationAddressResult = Address.Create(
            locationRequest.Address.Street,
            locationRequest.Address.City,
            locationRequest.Address.Country);
        if (locationAddressResult.IsFailure)
        {
            return locationAddressResult.Error;
        }

        Address locationAddress = locationAddressResult.Value;

        var locationTimezoneResult = Timezone.Create(locationRequest.Timezone);
        if (locationTimezoneResult.IsFailure)
        {
            return locationTimezoneResult.Error;
        }

        Timezone locationTimezone = locationTimezoneResult.Value;

        Locations locations = new Locations(locationId, locationName, locationTimezone, locationAddress,
            new List<DepartmentLocation>());

        return await _locationsRepository.Add(locations, cancellationToken);
    }
}