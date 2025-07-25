using Altinn.Profile.Integrations.Persistence;

using Wolverine.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories
{
    /// <summary>
    /// Adds support for transactional outbox for repositories that persist using EF Core
    /// </summary>
    public abstract class EFCoreTransactionalOutbox(IDbContextOutbox contextOutbox)
    {
        private readonly IDbContextOutbox _outbox = contextOutbox;

        /// <summary>
        /// Transactionally publishes a message and saves the dbContext changes
        /// </summary>
        protected async Task NotifyAndSave<TEvent>(ProfileDbContext databaseContext, Func<TEvent> eventRaiser, CancellationToken cancellationToken)
        {
            _outbox.Enroll(databaseContext);
            var eventForSending = eventRaiser();

            await _outbox.PublishAsync(eventForSending);
            await _outbox.SaveChangesAndFlushMessagesAsync(cancellationToken);
        }
    }
}
