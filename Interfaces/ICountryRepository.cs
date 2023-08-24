using PokemonReviewApp.Models;

namespace PokemonReviewApp.Interfaces
{
    public interface ICountryRepository
    {
        ICollection<Country> GetCountries();
        Country GetCountry(int id);
        bool CountryExist(int id);
        Country GetCountryByOwner(int ownerId);
        ICollection<Owner> GetOwnersFromCountry(int id);
        bool CreateCountry(Country country);
        bool UpdateCountry(Country country);
        bool DeleteCountry(Country country);
        bool Save();

    }
}
