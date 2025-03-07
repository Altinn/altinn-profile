namespace Altinn.Profile.Integrations.Entities
{
    /// <summary>
    /// The type of digital notification address
    /// </summary>
    public enum AddressType : int
    {
        /// <summary>
        /// Specify that address is an SMS address
        /// </summary>
        None = 0,

        /// <summary>
        /// Specify that address is an SMS address
        /// </summary>
        SMS = 1,

        /// <summary>
        /// Specify that address is an EMAIL address
        /// </summary>
        Email = 2
    }
}
