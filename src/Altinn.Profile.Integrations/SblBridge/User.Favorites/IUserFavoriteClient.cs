namespace Altinn.Profile.Integrations.SblBridge.User.Favorites
{
    /// <summary>
    /// Interface for managing user favorites.
    /// </summary>
    public interface IUserFavoriteClient
    {
        /// <summary>
        /// Updates the user's favorites based on the provided request.
        /// </summary>
        /// <param name="request">The request containing details of the favorite change.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateFavorites(FavoriteChangedRequest request);
    }
}
