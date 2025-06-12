using FluentValidation;
using NotesAPI.DTO;
using NotesAPI.Models;
using NotesAPI.Repositories;

namespace NotesAPI.Endpoints
{
    public static class LookupEndpoints
    {
        public static RouteGroupBuilder MapLookupApi(this RouteGroupBuilder group)
        {
            group.MapPost("/createLookup", async (CreateLookupDto dto, IValidator<CreateLookupDto> validator, ILookupRepository repo) =>
            {
                var validationResult = await validator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var lookup = new Lookup
                {
                    TypeId = dto.TypeId,
                    TypeCode = dto.TypeCode,
                    TypeName = dto.TypeName,
                };

                var created = await repo.CreateAsync(lookup);
                return Results.Created($"/getLookupById/{created.TypeId}", new { message = "Lookup created successfully", lookup = created });
            });


            group.MapGet("/getAllLookup", async (ILookupRepository repo) =>
            {
                var lookups = await repo.GetAllAsync();
                return Results.Ok(lookups);
            });

            group.MapGet("/getLookupById/{typeId}", async (string typeId, ILookupRepository repo) =>
            {
                var lookup = await repo.GetByIdAsync(typeId);
                return lookup is null ? Results.NotFound(new { message = "Lookup not found" }) : Results.Ok(lookup);
            });

            group.MapPut("/updateLookup/{typeId}", async (string typeId, UpdateLookupDto dto, HttpContext context, ILookupRepository repo) =>
            {
                var updated = await repo.UpdateAsync(typeId, new Lookup
                {
                    TypeCode = dto.TypeCode,
                    TypeName = dto.TypeName,
                });

                return updated is null ? Results.NotFound(new { message = "Lookup not found" }) : Results.Ok(new { message = "Lookup updated successfully", lookup = updated });
            });

            group.MapDelete("/deleteLookup/{typeId}", async (string typeId, ILookupRepository repo) =>
            {
                var success = await repo.DeleteAsync(typeId);
                return success ? Results.Ok(new { message = "Lookup deleted successfully" }) : Results.NotFound(new { message = "Lookup not found" });
            });

            return group;
        }
    }
}
