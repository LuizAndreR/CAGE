using FluentResults;

namespace CakeGestao.Domain.Exceptions;

public class ForbiddenError(string message) : Error(message);
