# Cleans up the Altinn resource list (resourceregistry .../resource/resourcelist).
#
# This is the complement of transform-with-AF-filter.jq: it keeps exactly the
# resources that script drops, i.e. everything that belongs to a test org, has an
# irrelevant resource type, or is not both visible and delegable. Keep the two
# predicates in sync.

def isTestOrg:
  # orgcode casing is inconsistent in the source data (both "acn" and "ACN" occur)
  (.hasCompetentAuthority.orgcode // "")
  | ascii_downcase
  | IN("acn", "bft", "ttd");

def isRelevantResourceType:
  .resourceType
  | IN("GenericAccessResource", "AltinnApp", "MigratedApp", "CorrespondenceService");

map(
  select(
    (
      (isTestOrg | not)
      and isRelevantResourceType
      and .visible == true
      and .delegable == true
    )
    | not
  )
  | {
      identifier,
      version,
      norwegianTitle: .title.nb,
      status,
      contactPoints,
      resourceReferences: (.resourceReferences // []),
      delegable,
      visible,
      competentAuthority: (
        .hasCompetentAuthority
        | { name: .name.nb, organization, orgcode }
      ),
      resourceType,
      availableForType,
      authorizationReference
    }
)
