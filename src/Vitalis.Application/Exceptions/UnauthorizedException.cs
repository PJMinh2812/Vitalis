namespace Vitalis.Application.Exceptions;

// Wrong credentials (VC-01) — distinct from ForbiddenException (account exists
// but isn't allowed to log in right now).
public class UnauthorizedException(string message) : Exception(message);
