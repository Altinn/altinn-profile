namespace Altinn.Profile.Core.User.ContactPoints
{
    /// <summary>
    /// Class describing the methods required for user contact point service
    /// </summary>
    public interface IUserContactPoints
    {
        /// <summary>
        /// Method for retriveing contact points for a user 
        /// </summary>
        /// <param name="nationalIdentityNumbers">A list of national identity numbers to lookup contact points for</param>
        /// <returns>The users' contact points and reservation status or a boolean if failure.</returns>
        Task<Result<UserContactPointsList, bool>> GetContactPoints(List<string> nationalIdentityNumbers);

        /// <summary>
        /// Method for retriveing information about the availability of contact points for a user 
        /// </summary>
        /// <param name="nationalIdentityNumbers">A list of national identity numbers to look up availability for</param>
        /// <returns>Information on the existense of the users' contact points and reservation status or a boolean if failure.</returns>
        Task<Result<UserContactPointAvailabilityList, bool>> GetContactPointAvailability(List<string> nationalIdentityNumbers);
    }
}
