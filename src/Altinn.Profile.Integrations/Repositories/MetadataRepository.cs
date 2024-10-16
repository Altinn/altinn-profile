using Altinn.Profile.Core;
using Altinn.Profile.Integrations.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Provides methods for handling metadata operations in the profile database.
/// </summary>
public class MetadataRepository : IMetadataRepository
{
    private readonly ProfileDbContext _context;
    private readonly ILogger<MetadataRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataRepository"/> class.
    /// </summary>
    /// <param name="logger">The logger instance used for logging operations.</param>
    /// <param name="context">The database context for accessing profile data.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="context"/> is null.</exception>
    public MetadataRepository(ILogger<MetadataRepository> logger, ProfileDbContext context)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Asynchronously retrieves the latest change number from the metadata.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object 
    /// with a <see cref="long"/> value on success, or a <see cref="bool"/> indicating failure.
    /// </returns>
    /// <exception cref="Exception">Thrown when an error occurs while retrieving the latest change number.</exception>
    public async Task<Result<long, bool>> GetLatestChangeNumberAsync()
    {
        try
        {
            var metaData = await _context.Metadata.FirstOrDefaultAsync();
            return metaData != null ? metaData.LatestChangeNumber : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the latest change number.");

            throw;
        }
    }
}
