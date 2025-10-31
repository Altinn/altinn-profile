namespace Altinn.Profile.Core.Utils
{
    /// <summary>
    /// Represents an optional value that may or may not be present.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public struct Optional<T>
    {
        /// <summary>
        /// Gets a value indicating whether a value is present.
        /// </summary>
        public bool HasValue { get; private set; }

        private T? _value;

        /// <summary>
        /// Gets or sets the value.
        /// Setting the value will mark <see cref="HasValue"/> as true.
        /// </summary>
        public T? Value
        {
            get => _value;
            set
            {
                HasValue = true;
                _value = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Optional{T}"/> class with no value.
        /// </summary>
        public Optional()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Optional{T}"/> class with the specified value.
        /// </summary>
        /// <param name="value">The value to initialize with.</param>
        public Optional(T? value)
        {
            HasValue = true;
            _value = value;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string representation of the value if present; otherwise, "(no value)".</returns>
        public override string ToString() => HasValue ? $"{_value}" : "(no value)";
    }
}
