namespace Altinn.Profile.Integrations.Register
{
    /// <summary>
    /// A http client for interactions with altinn-register
    /// </summary>
    public interface IRegisterClient
    {
        /// <summary>
        /// Optimistically get the main unit for an organization based on its organization number.
        /// </summary>
        /// <param name="orgNumber">Org number for the current organization</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<string> GetMainUnit(string orgNumber, CancellationToken cancellationToken);
    }
}
