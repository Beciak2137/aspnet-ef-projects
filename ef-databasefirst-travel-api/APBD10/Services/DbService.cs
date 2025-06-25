using APBD10.Models;
using APBD10.Data;
using APBD10.DTO;
using APBD10.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace APBD10.Services;

public interface IDbService
{
    Task<PagedTripsGetDto> GetTripsAsync(int page, int? pageSize);
    Task<IEnumerable<TripGetDto>> GetTripsAsync();
    Task DeleteClientByIdAsync(int clientId);
    Task AssignClientForTripAsync(int tripId, ClientPostDto client);

}

public class DbService(Apbd10Context data) : IDbService
{
    public async Task<PagedTripsGetDto> GetTripsAsync(int page, int? pageSize)
    {
        var trips = await data.Trips
            .Include(t => t.Countries)
            .Include(t => t.ClientTrips)
            .ThenInclude(ct => ct.IdClientNavigation)
            .ToListAsync();

        var tripDtos = trips
            .Select(trip => new TripGetDto()
            {
                Name = trip.Name,
                Description = trip.Description,
                DateFrom = trip.DateFrom,
                DateTo = trip.DateTo,
                MaxPeople = trip.MaxPeople,
                Countries = trip.Countries
                    .Select(country => new CountryGetDto() { Name = country.Name })
                    .ToList(),
                Clients = trip.ClientTrips
                    .Select(clientTrip => new ClientGetDto()
                    {
                        FirstName = clientTrip.IdClientNavigation.FirstName,
                        LastName = clientTrip.IdClientNavigation.LastName
                    })
                    .ToList()
            })
            .OrderByDescending(dto => dto.DateFrom)
            .ToList();

        int itemsPerPage = pageSize ?? 10;
        int totalItems = tripDtos.Count;
        int pageCount = (int)Math.Ceiling(totalItems / (double)itemsPerPage);

        if (page < 1 || itemsPerPage < 1)
            throw new BadRequestException("Invalid page size or number");

        if (page > pageCount)
            throw new BadRequestException("Not enough pages");

        var paginated = tripDtos
            .Skip((page - 1) * itemsPerPage)
            .Take(itemsPerPage)
            .ToList();

        return new PagedTripsGetDto()
        {
            PageNum = page,
            PageSize = itemsPerPage,
            AllPages = pageCount,
            Trips = paginated
        };
    }

    public async Task<IEnumerable<TripGetDto>> GetTripsAsync()
    {
        var trips = await data.Trips
            .Include(t => t.Countries)
            .Include(t => t.ClientTrips)
            .ThenInclude(ct => ct.IdClientNavigation)
            .ToListAsync();

        return trips
            .Select(t => new TripGetDto()
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Countries = t.Countries
                    .Select(c => new CountryGetDto() { Name = c.Name })
                    .ToList(),
                Clients = t.ClientTrips
                    .Select(ct => new ClientGetDto()
                    {
                        FirstName = ct.IdClientNavigation.FirstName,
                        LastName = ct.IdClientNavigation.LastName
                    })
                    .ToList()
            })
            .OrderByDescending(t => t.DateFrom)
            .ToList();
    }

    public async Task DeleteClientByIdAsync(int clientId)
    {
        //sprawdzamy czy klient istnieje
        var client = await data.Clients.FindAsync(clientId);
        if (client == null) 
            throw new NotFoundException($"Client with id: {clientId} doesn't exist");
        //sprawdzamy czy klient jest zapisany na jakąś wycieczke
        var clientTrip = await data.ClientTrips.FirstOrDefaultAsync(ct => ct.IdClient == clientId);
        if (clientTrip != null)
            throw new BadRequestException($"Client with id: {clientId} is assigned to at least one trip.");
        data.Clients.Remove(client);
        await data.SaveChangesAsync();
    }

    public async Task AssignClientForTripAsync(int tripId, ClientPostDto client)
    {
        await using var transaction = await data.Database.BeginTransactionAsync();

        try
        {
            //sprawdzamy czy klient istnieje
            var existingClient = await data.Clients.FirstOrDefaultAsync(c => c.Pesel == client.Pesel);
            if (existingClient != null)
                throw new BadRequestException("Client already exists.");
            
            //sprawdzamy czy wycieczka istnieje
            var trip = await data.Trips.FindAsync(tripId);
            if (trip == null)
                throw new NotFoundException($"Trip with ID {tripId} not found.");
            
            //sprawdzamy czy wycieczka jest w przeszłości
            if (trip.DateFrom <= DateTime.UtcNow)
                throw new BadRequestException("Trip already started.");
            
            //sprawdzamy czy klient jest już zapisany na tę wycieczkę
            var isAssigned = await data.ClientTrips
                .Include(ct => ct.IdClientNavigation)
                .AnyAsync(ct => ct.IdTrip == tripId && ct.IdClientNavigation.Pesel == client.Pesel);

            if (isAssigned)
                throw new BadRequestException($"Client is already assigned to trip {tripId}.");

            var newClient = new Client
            {
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                Telephone = client.Telephone,
                Pesel = client.Pesel
            };

            await data.Clients.AddAsync(newClient);
            await data.SaveChangesAsync();

            var clientTrip = new ClientTrip
            {
                IdClient = newClient.IdClient,
                IdTrip = tripId,
                RegisteredAt = DateTime.UtcNow,
                PaymentDate = client.PaymentDate
            };

            await data.ClientTrips.AddAsync(clientTrip);
            await data.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    
}