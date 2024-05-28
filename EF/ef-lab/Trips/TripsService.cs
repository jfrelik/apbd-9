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
}