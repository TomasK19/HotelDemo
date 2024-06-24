using HotelDemo.Data;
using HotelDemo.Mediator.Commands.Email;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace HotelDemo.Mediator.Handlers.Email;

public class VerifyEmailCommandHandler(HotelBookingContext context) : IRequestHandler<VerifyEmailCommand, Unit>
{
    public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.SingleOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || user.VerificationCode != request.VerificationCode)
            throw new UnauthorizedAccessException("Invalid verification code");

        user.IsVerified = true;
        user.VerificationCode = null;

        context.Users.Update(user);
        await context.SaveChangesAsync();
        return Unit.Value;
    }
}
