using System.Collections.Generic;

namespace RefactorNeeded.Commons.ErrorHandling
{
    public abstract class AppError
    {
    }

    public class NotValidError : AppError
    {
        public string Code { get; }

        public string Message { get; }

        public IReadOnlyDictionary<string, string> Errors { get; }

        public NotValidError(string code, string message, Dictionary<string, string>? errors = null)
        {
            Code = code;
            Message = message;
            Errors = errors ?? new Dictionary<string, string>();
        }

        public NotValidError(DomainError domainError)
        {
            Code = domainError.Code;
            Message = domainError.Message;
            Errors = domainError.Errors;
        }
    }

    public class NotFoundError : AppError
    {
        public string Message { get; }

        public NotFoundError(string message)
        {
            Message = message;
        }
    }

    public class NotAuthorizedError : AppError
    {
        public string Message { get; }

        public NotAuthorizedError(string message)
        {
            Message = message;
        }
    }

    public class DomainError : NotValidError
    {
        public DomainError(string code, string message) : base(code, message)
        {
        }
    }
}