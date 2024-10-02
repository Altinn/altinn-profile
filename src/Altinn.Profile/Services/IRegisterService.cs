using System.Collections.Generic;
using System.Threading.Tasks;

using Altinn.Profile.Models;
using Altinn.Profile.Repositories;

namespace Altinn.Profile.Services
{
    /// <summary>
    /// Register service for handling register data
    /// </summary>
    public interface IRegisterService
    {
        /// <summary>
        /// Gets the by national identity number asynchronous.
        /// </summary>
        /// <param name="nationalIdentityNumber">The national identity number.</param>
        /// <returns></returns>
        Task<Register> GetUserContactPointAsync(string nationalIdentityNumber);

        /// <summary>
        /// Gets the by national identity number asynchronous.
        /// </summary>
        /// <param name="nationalIdentityNumber">The national identity number.</param>
        /// <returns></returns>
        Task<IEnumerable<Register>> GetUserContactPointAsync(IEnumerable<string> nationalIdentityNumber);
    }

    /// <summary>
    /// Register service for handling register data
    /// </summary>
    /// <seealso cref="Altinn.Profile.Services.IRegisterService" />
    public class RegisterService : IRegisterService
    {
        private readonly IRegisterRepository _registerRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterService"/> class.
        /// </summary>
        /// <param name="registerRepository">The register repository.</param>
        public RegisterService(IRegisterRepository registerRepository)
        {
            _registerRepository = registerRepository;
        }

        /// <summary>
        /// Gets the by national identity number asynchronous.
        /// </summary>
        /// <param name="nationalIdentityNumber">The national identity number.</param>
        /// <returns></returns>
        public async Task<Register> GetUserContactPointAsync(string nationalIdentityNumber)
        {
            return await _registerRepository.GetUserContactPointAsync(nationalIdentityNumber);
        }

        /// <summary>
        /// Gets the by national identity number asynchronous.
        /// </summary>
        /// <param name="nationalIdentityNumber">The national identity number.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Register>> GetUserContactPointAsync(IEnumerable<string> nationalIdentityNumber)
        {
            return await _registerRepository.GetUserContactPointAsync(nationalIdentityNumber);
        }
    }
}
