namespace Animato.Sso.Application.Features.Claims.DTOs;
using System;

public static class ClaimExtensions
{
    public static CreateClaimModel ValidateAndSanitize(this CreateClaimModel claim)
    {
        if (claim is null)
        {
            throw new ArgumentNullException(nameof(claim));
        }

        if (string.IsNullOrEmpty(claim.Name))
        {
            throw new Exceptions.ValidationException(
                Exceptions.ValidationException.CreateFailure(nameof(claim.Name)
                , $"Name cannot be empty"));
        }

        if (string.IsNullOrEmpty(claim.Description))
        {
            claim.Description = "";
        }

        return claim;
    }

    public static Domain.Entities.Claim ApplyTo(this CreateClaimModel model, Domain.Entities.Claim claim)
    {
        claim.Name = model.Name;
        claim.Description = model.Description;
        return claim;
    }
}
