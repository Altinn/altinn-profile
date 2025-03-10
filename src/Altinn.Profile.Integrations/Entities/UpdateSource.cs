namespace Altinn.Profile.Integrations.Entities
{
    /// <summary>
    ///  UpdateOrigin is used to specify the the changes of the UnitNotificationEndPoint 
    /// </summary>
    public enum UpdateSource : int
    {
        /// <summary>
        /// UpdateOrigin is None
        /// </summary>
        None = 0,

        /// <summary>
        /// UpdateOrigin is Altinn
        /// </summary>
        Altinn = 1,

        /// <summary>
        /// UpdateOrigin Type is KoFuVi
        /// </summary>
        KoFuVi = 2,
    }
}
