using EF.Trips.Context;
using EF.Trips.ResponseModels;
using Microsoft.EntityFrameworkCore;

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

        var trips = await _context.Trips
            .Where(t => query == null || t.Name.Contains(query))
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
            AllPages = await _context.Trips.CountAsync() / pageSize.Value,
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
}