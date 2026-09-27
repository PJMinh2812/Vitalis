namespace Vitalis.Application.Exceptions;

// Account exists and credentials may even be correct, but it's not allowed to
// act right now — is_active = 0, or temporarily locked out (VC-01).
public class ForbiddenException(string message) : Exception(message);
