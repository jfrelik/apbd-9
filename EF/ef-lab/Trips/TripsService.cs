using EF.Trips.Context;
using EF.Trips.ResponseModels;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EF.Trips;

public class TripsService : ITripsService
{
    private readonly TripsDbContext _context;

    public TripsService(TripsDbContext context)
    {
        _context = context;
    }

    public async Task<AllTripInfo> GetTrips(string? query, int? pageNum, int? pageSize)
    {
        pageSize ??= 10;
        pageNum ??= 1;

        if (pageNum < 1)
            throw new ArgumentException("Page number is invalid");

        if (pageSize < 1)
            throw new ArgumentException("Page size is invalid");

        var tripsCount = await _context.Trips.Where(t => query == null || t.Name.Contains(query)).CountAsync();
        var allPages = tripsCount / pageSize.Value + (tripsCount % pageSize.Value == 0 ? 0 : 1);

        if (pageNum > allPages)
            throw new ArgumentException("Page number is too high");

        if (tripsCount == 0)
            throw new DataException("No trips found");

        var trips = await _context.Trips
            .Where(t => query == null || t.Name.Contains(query))
            .OrderByDescending(t => t.DateFrom)
            .Skip((pageNum.Value - 1) * pageSize.Value)
            .Take(pageSize.Value)
            .Include(t => t.IdCountries)
            .Include(t => t.ClientTrips)
            .ThenInclude(ct => ct.IdClientNavigation)
            .ToListAsync();

        return new AllTripInfo
        {
            PageNum = pageNum.Value,
            PageSize = pageSize.Value,
            AllPages = allPages,
            Trips = trips.Select(t => new Trip
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Countries = t.IdCountries.Select(c => new Country
                {
                    Name = c.Name
                }).ToList(),
                Clients = t.ClientTrips.Select(ct => new Client
                {
                    FirstName = ct.IdClientNavigation.FirstName,
                    LastName = ct.IdClientNavigation.LastName,
                }).ToList(),
            }).ToList(),
        };
    }

    public async Task<String> DeleteClient(int idClient)
    {
        var client = await _context.Clients.FindAsync(idClient);
        if (client == null)
            throw new DataException("Client not found");

        var clientTrips = await _context.ClientTrips.Where(ct => ct.IdClient == idClient).ToListAsync();
        if (clientTrips.Count > 0)
            throw new DataException("Client has trips and cannot be deleted");

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return "Client deleted";
    }

    public async Task<String> AddClientToTrip(int idTrip, NewClientDetails clientDetails)
    {
        var trip = await _context.Trips.Where(t => t.IdTrip == idTrip).FirstAsync();
        if (trip == null)
            throw new DataException("Trip not found");

        if (trip.DateFrom < DateTime.Now)
            throw new DataException("Trip already started");
        
        var clientTripWithPesel = await _context.ClientTrips.FirstOrDefaultAsync(ct =>
            ct.IdTrip == idTrip && ct.IdClientNavigation.Pesel == clientDetails.Pesel);
        if (clientTripWithPesel != null)
            throw new DataException("Client with this PESEL is already at the trip");

        var clientWithPesel = await _context.Clients.FirstOrDefaultAsync(c => c.Pesel == clientDetails.Pesel);
        if (clientWithPesel != null)
            throw new DataException("Client with this PESEL already exists");

        var client = new Models.Client
        {
            FirstName = clientDetails.FirstName,
            LastName = clientDetails.LastName,
            Email = clientDetails.Email,
            Telephone = clientDetails.Telephone,
            Pesel = clientDetails.Pesel,
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        var clientTrip = new Models.ClientTrip
        {
            IdClient = client.IdClient,
            IdTrip = idTrip,
            PaymentDate = clientDetails.PaymentDate,
            RegisteredAt = DateTime.Now,
        };

        _context.ClientTrips.Add(clientTrip);
        await _context.SaveChangesAsync();

        return "Client added to trip";
    }
}