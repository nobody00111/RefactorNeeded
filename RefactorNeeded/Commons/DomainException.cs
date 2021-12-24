using System;

namespace RefactorNeeded.Commons
{
    public class DomainException : Exception
    {
        public DomainException(string message)
            : base(message)
        {
        }
    }
}