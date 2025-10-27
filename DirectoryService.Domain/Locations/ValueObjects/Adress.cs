using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.Locations.ValueObjects;

public record Address
{
    public string Street { get; }
    public string City { get; }
    public string Country { get; }

    private Address(string street, string city, string country)
    {
        Street = street;
        City = city;
        Country = country;
    }

    public static Result<Address, Error> Create(string street, string city, string country)
    {
        if(string.IsNullOrWhiteSpace(street))
            return Error.Validation(null, "Street is required");
        if(street.Length > LenghtConstants.LENGTH3)
            return Error.Validation("length.is.invalid", "Location street cannot be more than 3 characters");

        if(string.IsNullOrWhiteSpace(city))
            return Error.Validation(null, "City is required");
        if(city.Length > LenghtConstants.LENGTH3)
            return Error.Validation("length.is.invalid", "Location city cannot be more than 3 characters");

        if(string.IsNullOrWhiteSpace(country))
            return Error.Validation(null, "Country is required");
        if(country.Length > LenghtConstants.LENGTH3)
            return Error.Validation("length.is.invalid", "Location country cannot be more than 3 characters");

        Address adress = new(street, city, country);

        return Result.Success<Address, Error>(adress);
    }
}