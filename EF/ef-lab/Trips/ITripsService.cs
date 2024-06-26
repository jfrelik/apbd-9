using EF.Trips.ResponseModels;

namespace EF.Trips;

public interface ITripsService
{
    Task<AllTripInfo> GetTrips(string? query, int? pageNum, int? pageSize);

    Task<String> DeleteClient(int idClient);
    
    Task<String> AddClientToTrip(int idTrip, NewClientDetails clientDetails);
}