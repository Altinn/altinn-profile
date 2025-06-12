using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.Profile.Integrations.Register
{
    /// <summary>
    /// Configuration object used to hold settings for all Altinn Register integrations.
    /// </summary>
    public class RegisterSettings
    {
        /// <summary>
        /// Gets or sets the url for the register API
        /// </summary>
        public string ApiRegisterEndpoint { get; set; } = string.Empty;
    }
}
