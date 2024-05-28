using System.Data;

namespace EF.Trips;

public static class Configuration
{
    public static void RegisterEndpoinsForTrips(this IEndpointRouteBuilder app)
    {
        app.MapGet("api/trips", async (ITripsService service, String? query, int? pageNum, int? pageSize) =>
        {
            try
            {
                var trips = await service.GetTrips(query, pageNum, pageSize);
                return Results.Ok(trips);
            }
            catch (DataException e)
            {
                return Results.NotFound(e.Message);
            }
            catch (ArgumentException e)
            {
                return Results.BadRequest(e.Message);
            }
            catch (Exception e)
            {
                return Results.Problem(e.Message);
            }
        });
        
        app.MapDelete("api/clients/{idClient}", async (ITripsService service, int idClient) =>
        {
            try
            {
                var result = await service.DeleteClient(idClient);
                return Results.Ok(result);
            }
            catch (DataException e)
            {
                return Results.NotFound(e.Message);
            }
            catch (Exception e)
            {
                return Results.Problem(e.Message);
            }
        });
    }
}