﻿using System;
using System.Collections.Generic;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Tests.Testdata;

/// <summary>
/// Provides test data for notification addresses for organizations.
/// </summary>
public static class OrganizationNotificationAddressTestData
{
    /// <summary>
    /// Gets a list of notificationAddresses.
    /// </summary>
    /// <returns>A list of <see cref="Register"/> objects containing test data.</returns>
    public static (List<OrganizationDE> Organizations, List<NotificationAddressDE> NotificationAddresses) GetNotificationAddresses()
    {
        var organizations = new List<OrganizationDE>()
        {
            new()
            {
                RegistryOrganizationNumber = "123456789",
                RegistryOrganizationId = 1
            },
            new()
            {
                RegistryOrganizationNumber = "987654321",
                RegistryOrganizationId = 2
            },
            new()
            {
                RegistryOrganizationNumber = "999999999",
                RegistryOrganizationId = 3
            }
        };
        var notificationAddresses = new List<NotificationAddressDE>()
        {
            new()
            {
                AddressType = AddressType.Email,
                Address = "test.email",
                Domain = "test.no",
                FullAddress = "test.email@test.no",
                IsSoftDeleted = false,
                UpdateSource = UpdateSource.KoFuVi,
                HasRegistryAccepted = true,
                RegistryUpdatedDateTime = DateTime.Now.AddDays(-1),
                RegistryID = Guid.NewGuid().ToString("N"),
                RegistryOrganizationId = 1,
                NotificationAddressID = 1
            },
            new()
            {
                AddressType = AddressType.SMS,
                Address = "98765432",
                Domain = "+47",
                FullAddress = "+4798765432",
                IsSoftDeleted = false,
                UpdateSource = UpdateSource.KoFuVi,
                HasRegistryAccepted = true,
                RegistryUpdatedDateTime = DateTime.Now.AddDays(-2),
                RegistryID = Guid.NewGuid().ToString("N"),
                RegistryOrganizationId = 1,
                NotificationAddressID = 2
            },
            new()
            {
                AddressType = AddressType.Email,
                Address = "test.ceo",
                Domain = "test.no",
                FullAddress = "test.email@test.no",
                IsSoftDeleted = false,
                HasRegistryAccepted = true,
                UpdateSource = UpdateSource.KoFuVi,
                RegistryUpdatedDateTime = DateTime.Now.AddDays(-10),
                RegistryID = Guid.NewGuid().ToString("N"),
                RegistryOrganizationId = 1,
                NotificationAddressID = 3
            },
            new()
            {
                AddressType = AddressType.Email,
                Address = "test.other",
                Domain = "company.no",
                FullAddress = "test.other@company.no",
                IsSoftDeleted = false,
                UpdateSource = UpdateSource.KoFuVi,
                HasRegistryAccepted = true,
                RegistryUpdatedDateTime = DateTime.Now.AddDays(-3),
                RegistryID = Guid.NewGuid().ToString("N"),
                RegistryOrganizationId = 2,
                NotificationAddressID = 4
            },
            new()
            {
                AddressType = AddressType.SMS,
                Address = "98765433",
                Domain = "+47",
                FullAddress = "+4798765433",
                IsSoftDeleted = false,
                HasRegistryAccepted = false,
                UpdateSource = UpdateSource.Altinn,
                RegistryUpdatedDateTime = DateTime.Now.AddDays(-20),
                RegistryID = "27ae0c8bea1f4f02a974c10429c32758",
                RegistryOrganizationId = 2,
                NotificationAddressID = 5
            },
            new()
            {
                AddressType = AddressType.SMS,
                Address = "98765434",
                Domain = "+47",
                FullAddress = "+4798765434",
                IsSoftDeleted = true,
                HasRegistryAccepted = true,
                UpdateSource = UpdateSource.KoFuVi,
                RegistryUpdatedDateTime = DateTime.Now.AddDays(-25),
                RegistryID = "27ae0c8bea1f4f02a974c10429c32759",
                RegistryOrganizationId = 3,
                NotificationAddressID = 6
            },
        };
            
        return (organizations, notificationAddresses);
    }
}
