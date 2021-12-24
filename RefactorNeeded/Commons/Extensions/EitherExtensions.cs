using System;
using RefactorNeeded.Commons.ErrorHandling;

namespace RefactorNeeded.Commons.Extensions
{
    public static class EitherExtensions
    {
        public static Either<Success, AppError> ToAppResult<T>(this Either<T, DomainError> result)
        {
            return result.IsSuccess ? Success.Value : new NotValidError(result.Error.Code, result.Error.Message);
        }

        public static void ThrowIfError<T>(this Either<T, DomainError> result)
        {
            if (!result.IsSuccess) throw new InvalidOperationException(result.Error.Code);
        }

        public static Either<U, AppError> ToAppResult<T, U>(this Either<T, DomainError> result,
            Func<T, U> dataTransform)
        {
            return result.IsSuccess
                ? dataTransform(result.Data)
                : new NotValidError(result.Error.Code, result.Error.Message);
        }
    }
}