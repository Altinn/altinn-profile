namespace Altinn.Profile.Integrations.Entities
{
    /// <summary>
    /// AddressType is used to define type of address
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
