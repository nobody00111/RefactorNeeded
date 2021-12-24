namespace RefactorNeeded.Commons.ErrorHandling
{
    public class Either<TData, TError>
    {
        public TData Data { get; }

        public TError Error { get; }

        public bool IsSuccess { get; }

        public static Either<TData, TError> Failure(TError error)
        {
            return new(false, default!, error);
        }

        public static Either<TData, TError> Success(TData data)
        {
            return new(true, data, default!);
        }

        private Either(bool isSuccess, TData data, TError error)
        {
            IsSuccess = isSuccess;
            Error = error;
            Data = data;
        }

        public static implicit operator Either<TData, TError>(TData data)
        {
            return Success(data);
        }

        public static implicit operator Either<TData, TError>(TError error)
        {
            return Failure(error);
        }
    }
}