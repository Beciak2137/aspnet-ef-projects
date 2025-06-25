using CW7_S30595.Exceptions;
using CW7_S30595.Models;
using CW7_S30595.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace CW7_S30595.Services;

public interface IDbService
{
    public Task<IEnumerable<TripGetDTO>> GetTripsAsync();
    public Task<IEnumerable<ClientTripGetDTO>> GetClientTripsAsync(int id);
    public Task<Client> CreateClientAsync(ClientCreateDTO client);
    public Task DeleteClientFromTripAsync(int id, int tripId);
    public Task RegisterClientForTripAsync(int id, int tripId);
    
}
public class DbService(IConfiguration config) : IDbService
{
    private readonly string? _connectionString = config.GetConnectionString("Default");

    public async Task<IEnumerable<TripGetDTO>> GetTripsAsync()
    {
        //wszystkie wycieczki wyciąga i kraj 
        var result = new List<TripGetDTO>();
        await using var connection = new SqlConnection(_connectionString);
        
        const string sql = "SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.Name AS CountryName" +
                           " FROM Trip t LEFT JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip" +
                           " LEFT JOIN Country c ON ct.IdCountry = c.IdCountry";
        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new TripGetDTO
            {
                IdTrip = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                DateFrom = reader.GetDateTime(3),
                DateTo = reader.GetDateTime(4),
                MaxPeople = reader.GetInt32(5),
                Country = reader.IsDBNull(6) ? null : reader.GetString(6)
            });
        }
        return result;
    }

    public async Task<IEnumerable<ClientTripGetDTO>> GetClientTripsAsync(int id)
    {
        //wyciąga wycieczki klientów o określonym id i sprawdza czy klient istenje 
        var result = new List<ClientTripGetDTO>();
        using var connection = new SqlConnection(_connectionString);
        const string sql = "SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.Name AS CountryName" +
                           " FROM Trip t INNER JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip" +
                           " LEFT JOIN Country_Trip cct ON t.IdTrip = cct.IdTrip LEFT JOIN Country c ON cct.IdCountry = c.IdCountry" +
                           " WHERE ct.IdClient = @Id";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            throw new NotFoundException("Client not found");
        }

        while (await reader.ReadAsync())
        {
            result.Add(new ClientTripGetDTO
            {
                IdTrip = reader.GetInt32(0),
                RegisteredAt = reader.GetInt32(6),
                PaymentDate = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                DateFrom = reader.GetDateTime(3),
                DateTo = reader.GetDateTime(4),
            });
        }

        if (result.Count == 0)
        {
            throw new NotFoundException("No trips found");
        }
        return result;
    }

    public async Task<Client> CreateClientAsync(ClientCreateDTO client)
    {
        //dodaje nowego klienta
        await using var connection = new SqlConnection(_connectionString);
        const string sql = "INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel) VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel); SELECT SCOPE_IDENTITY()";
        await using var command = new SqlCommand(sql,connection);
        command.Parameters.AddWithValue("@FirstName", client.FirstName);
        command.Parameters.AddWithValue("@LastName", client.LastName);
        command.Parameters.AddWithValue("@Email", client.Email);
        command.Parameters.AddWithValue("@Telephone", client.Telephone);
        command.Parameters.AddWithValue("@Pesel", client.Pesel);
        var id = Convert.ToInt32(await command.ExecuteScalarAsync());
        return new Client
        {
            IdClient = id,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            Telephone = client.Telephone,
            Pesel = client.Pesel
        };
    }
    

    public async Task DeleteClientFromTripAsync(int id, int tripId)
    {
        //usuwa klinta z wycieczki 
        await using var connection = new SqlConnection(_connectionString);
        const string sql = "DELETE FROM Client_Trip WHERE IdClient = @ClientId AND IdTrip = @TripId";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ClientId", id);
        command.Parameters.AddWithValue("@TripId", tripId);
        var numOfRows = await command.ExecuteNonQueryAsync();
        if (numOfRows == 0)
        {
            throw new NotFoundException($"Client {id} has no trip with id {tripId}");
        }
    }

    public async Task RegisterClientForTripAsync(int id, int tripId)
    {
        //rejestruje klienta na wycieczke i sprawdza czy klient istnieje, czy wyceczka istnieje, czy nie przkroczyła max osóv
        await using var connection = new SqlConnection(_connectionString);
        var clientExist = new SqlCommand("SELECT 1 FROM Client WHERE IdClient = @ClientId", connection);
        clientExist.Parameters.AddWithValue("@ClientId", id);
        if (clientExist.ExecuteScalar() == null)
            throw new NotFoundException($"Client {id} does not exist");
        var tripexist = new SqlCommand("SELECT 1 FROM Trip WHERE IdTrip = @TripId", connection);
        tripexist.Parameters.AddWithValue("@TripId", tripId);
        if (tripexist.ExecuteScalar() == null)
        {
            throw new NotFoundException($"Trip {tripId} does not exist");
        }
        var maxPeopleSql = new SqlCommand("SELECT MaxPeople FROM Trip WHERE IdTrip = @TripId ", connection);
        maxPeopleSql.Parameters.AddWithValue("@TripId", tripId);
        var maxPeople2 = maxPeopleSql.ExecuteScalar();
        int maxPeople = Convert.ToInt32(maxPeople2);
        var registeredPeople = new SqlCommand("SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @TripId", connection);
        registeredPeople.Parameters.AddWithValue("@TripId", tripId);
        int numOfPeople;
        if (registeredPeople.ExecuteScalar() == null)
        {
            numOfPeople = 0;
        }
        else
        {
            numOfPeople = Convert.ToInt32(registeredPeople.ExecuteScalar());
        }
        if(numOfPeople > maxPeople)
            throw new DifferentException("Max people exceeded");
        var ifRegistered = new SqlCommand("SELECT 1 FROM Client_Trip WHERE IdClient = @ClientId AND IdTrip = @TripId", connection);
        ifRegistered.Parameters.AddWithValue("@ClientId", id);
        ifRegistered.Parameters.AddWithValue("@TripId", tripId);
        if (ifRegistered.ExecuteScalar() != null)
        {
            throw new DifferentException($"Client {id} has already been registered for trip {tripId}");
        }
        var sql = new SqlCommand("INSERT INTO Client_Trip (IdClient, IdTrip,RegisteredAt) VALUES (@ClientId, @TripId, @RegisteredAt)", connection);
        sql.Parameters.AddWithValue("@ClientId", id);
        sql.Parameters.AddWithValue("@TripId", tripId);
        sql.Parameters.AddWithValue("@RegisteredAt", DateTime.Now.ToString("yyyy-MM-dd"));
        await sql.ExecuteNonQueryAsync();
    }
}