namespace Vitalis.Application.Exceptions;

// e.g. a slot just taken by someone else between page load and submit (VC-03)
public class ConflictException(string message) : Exception(message);
