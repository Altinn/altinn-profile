using System.Collections.Generic;
using System.Threading.Tasks;
using Altinn.Profile.Models;
using Altinn.Profile.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Controllers
{
    /// <summary>
    /// Handles the presentation of unhandled exceptions during the execution of a request.
    /// </summary>
    [ApiController]
    [Route("profile/api/v1/user")]
    public class RegisterController : ControllerBase
    {
        private readonly IRegisterService _registerService;

        /// <summary>
        /// Values the tuple.
        /// </summary>
        /// <typeparam name="IRegisterService">The type of the register service.</typeparam>
        /// <returns></returns>
        public RegisterController(IRegisterService registerService)
        {
            _registerService = registerService;
        }

        /// <summary>
        /// Gets the user contact points by a collection of national identity numbers.
        /// </summary>
        /// <param name="nationalIdentityNumbers">A collection of national identity numbers.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of user contact points.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<UserContactPoint>>> GetByNationalIdentityNumbersAsync([FromBody] IEnumerable<string> nationalIdentityNumbers)
        {
            var data = await _registerService.GetUserContactPointAsync(nationalIdentityNumbers);
            if (data == null)
            {
                return NotFound();
            }

            var result = new List<UserContactPoint>();
            foreach (var item in data)
            {
                result.Add(new UserContactPoint
                {
                    Reservation = item.Reservation,
                    EmailAddress = item.EmailAddress,
                    LanguageCode = item.LanguageCode,
                    NationalIdentityNumber = item.FnumberAk,
                    MobilePhoneNumber = item.MobilePhoneNumber
                });
            }

            return Ok(result);
        }
    }
}
