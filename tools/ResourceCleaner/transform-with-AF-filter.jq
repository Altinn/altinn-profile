# Cleans up the Altinn resource list (resourceregistry .../resource/resourcelist).
#
# Keeps only resources that are in active use, then reshapes them into a slimmer
# structure.

def isTestOrg:
  # orgcode casing is inconsistent in the source data (both "acn" and "ACN" occur)
  (.hasCompetentAuthority.orgcode // "")
  | ascii_downcase
  | IN("acn", "bft");

def isRelevantResourceType:
  .resourceType
  | IN("GenericAccessResource", "AltinnApp", "MigratedApp", "CorrespondenceService");

map(
  select(
    (isTestOrg | not)
    and isRelevantResourceType
  )
  | {
      identifier,
      version,
      norwegianTitle: .title.nb,
      status,
      contactPoints,
      resourceReferences: (.resourceReferences // []),
      competentAuthority: (
        .hasCompetentAuthority
        | { name: .name.nb, organization, orgcode }
      ),
      resourceType,
      availableForType,
      authorizationReference
    }
)
