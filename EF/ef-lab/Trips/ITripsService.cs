using EF.Trips.ResponseModels;

namespace EF.Trips;

public interface ITripsService
{
    Task<AllTripInfo> GetTrips(string? query, int? pageNum, int? pageSize);
}