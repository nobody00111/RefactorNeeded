namespace RefactorNeeded.Commons.ErrorHandling
{
    public class Option<TValue>
    {
        public TValue Value { get; }

        public bool HasValue { get; }

        public static Option<TValue> Some(TValue value)
        {
            return new(true, value);
        }

        public static Option<TValue> None()
        {
            return new(false, default!);
        }

        private Option(bool hasValue, TValue value)
        {
            HasValue = hasValue;
            Value = value;
        }

        public Either<TValue, TError> ToEither<TError>(TError error)
        {
            return HasValue ? Either<TValue, TError>.Failure(error) : Either<TValue, TError>.Success(Value);
        }

        public static implicit operator Option<TValue>(TValue value)
        {
            return Some(value);
        }
    }
}