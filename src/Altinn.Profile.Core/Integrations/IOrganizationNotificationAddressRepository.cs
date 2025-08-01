﻿using Altinn.Profile.Core.OrganizationNotificationAddresses;

namespace Altinn.Profile.Core.Integrations;

/// <summary>
/// Defines a repository for interacting with notification addresses of organizations.
/// </summary>
public interface IOrganizationNotificationAddressRepository
{
    /// <summary>
    /// Fetches a single organization's notification addresses
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> with an organization as value.</returns>
    Task<Organization?> GetOrganizationAsync(string organizationNumber, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches organizations notification addresses
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> with a collection of organizations as value.</returns>
    Task<IEnumerable<Organization>> GetOrganizationsAsync(List<string> organizationNumbers, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new notification address for an organization
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> with the notification address as value.</returns>
    Task<NotificationAddress> CreateNotificationAddressAsync(string organizationNumber, NotificationAddress notificationAddress, string registryId);

    /// <summary>
    /// Delete a notification address for an organization
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> with the notification address as value.</returns>
    Task<NotificationAddress> DeleteNotificationAddressAsync(int notificationAddressId);

    /// <summary>
    /// Updates an existing notification address for an organization
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> with the notification address as value.</returns>
    Task<NotificationAddress> UpdateNotificationAddressAsync(NotificationAddress notificationAddress, string registryId);
}
