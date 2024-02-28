using System.Collections.Generic;
using System.Threading.Tasks;

using Altinn.Profile.Models;

namespace Altinn.Profile.Services.Interfaces
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
        /// <returns>The users' contact points and reservation status</returns>
        Task<UserContactPointsList> GetContactPoints(List<string> nationalIdentityNumbers);

        /// <summary>
        /// Method for retriveing information about the availability of contact points for a user 
        /// </summary>
        /// <param name="nationalIdentityNumbers">A list of national identity numbers to look up availability for</param>
        /// <returns>Information on the existense of the users' contact points and reservation status</returns>
        Task<UserContactPointAvailabilityList> GetContactPointAvailability(List<string> nationalIdentityNumbers);
    }
}
