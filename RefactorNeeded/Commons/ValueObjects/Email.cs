namespace RefactorNeeded.Commons.ValueObjects
{
    public class Email : ValueObject
    {
        public string Value { get; }

        public static Email? Create(string? value)
        {
            if (string.IsNullOrEmpty(value)) return null;

            return new Email(value);
        }

        public Email(string value)
        {
            Value = value;
        }
    }
}