namespace Altinn.Profile.Core.User.ContactPoints;

/// <summary>
/// Class describing the methods required for user contact point service
/// </summary>
public interface IUserContactPointsService
{
    /// <summary>
    /// Method for retriveing contact points for a user 
    /// </summary>
    /// <param name="nationalIdentityNumbers">A list of national identity numbers to lookup contact points for</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The users' contact points and reservation status or a boolean if failure.</returns>
    Task<UserContactPointsList> GetContactPoints(List<string> nationalIdentityNumbers, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the self-identified users' contact points associated with the specified external identities.
    /// </summary>
    /// <param name="externalIdentities">A list of external identities for which to retrieve contact points. External identities must be in urn-format: urn:altinn:person:idporten-email:: 
    /// as the namespace and the email address as the value part of the urn
    /// elements. Otherwise, the identifier will be discarded</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="SelfIdentifiedUserContactPointsList"/> with the contact points for the external identities. If no contact points
    /// are found, the list will be empty.</returns>
    Task<SelfIdentifiedUserContactPointsList> GetSiContactPoints(List<string> externalIdentities, CancellationToken cancellationToken);

    /// <summary>
    /// Method for retriveing information about the availability of contact points for a user 
    /// </summary>
    /// <param name="nationalIdentityNumbers">A list of national identity numbers to look up availability for</param>
    /// <returns>Information on the existense of the users' contact points and reservation status or a boolean if failure.</returns>
    Task<UserContactPointAvailabilityList> GetContactPointAvailability(List<string> nationalIdentityNumbers);
}
