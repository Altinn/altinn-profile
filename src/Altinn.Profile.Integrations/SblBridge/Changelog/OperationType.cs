using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.Profile.Integrations.SblBridge.Changelog
{
    /// <summary>
    /// Represents the different type of changes that can occur. Typically insert, update and delete operation.
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// A profile data element was created.
        /// </summary>
        Insert,

        /// <summary>
        /// A profile data element was updated.
        /// </summary>
        Update,

        /// <summary>
        /// A profile data element was deleted.
        /// </summary>
        Delete
    }
}
