#!/usr/bin/env bash
#
# Derive the deployed base image reference from a Dockerfile.
#
# The scanned artifact is the final build stage (docker build with no --target),
# so the *last* FROM line identifies the base image that actually ships. This
# script parses that line and derives the floating channel tag Microsoft
# publishes (major.minor + OS suffix), which always points at the newest patch.
# Nothing is hardcoded, so bumping the Dockerfile to a new .NET or OS version is
# picked up automatically.
#
# Usage: derive-base-image.sh <Dockerfile>
# Output: KEY=VALUE lines (suitable for appending to $GITHUB_OUTPUT).
#
#   BASE_REPO     e.g. mcr.microsoft.com/dotnet/aspnet
#   BASE_TAG      e.g. 10.0.9-alpine3.23
#   BASE_DIGEST   e.g. sha256:...            (empty if the FROM line is not pinned)
#   BASE_VERSION  e.g. 10.0.9
#   BASE_CHANNEL  e.g. 10.0
#   OS_SUFFIX     e.g. alpine3.23            (empty if the tag has no OS suffix)
#   FLOATING_TAG  e.g. 10.0-alpine3.23

set -euo pipefail

dockerfile="${1:?usage: derive-base-image.sh <Dockerfile>}"

if [[ ! -f "$dockerfile" ]]; then
  echo "derive-base-image.sh: file not found: $dockerfile" >&2
  exit 1
fi

# Last FROM line = the final stage = the image that gets tagged and scanned.
from_line=$(grep -iE '^[[:space:]]*FROM[[:space:]]' "$dockerfile" | tail -n1)
if [[ -z "$from_line" ]]; then
  echo "derive-base-image.sh: no FROM line found in $dockerfile" >&2
  exit 1
fi

# Strip the leading "FROM" keyword and any trailing "AS <stage>" alias, then the
# only remaining token is the image reference (repo:tag@digest).
ref=$(printf '%s\n' "$from_line" \
  | sed -E 's/^[[:space:]]*[Ff][Rr][Oo][Mm][[:space:]]+//; s/[[:space:]]+[Aa][Ss][[:space:]]+.*$//' \
  | tr -d '[:space:]')

# Split off the optional @sha256:... digest.
digest=""
case "$ref" in
  *@*) digest="${ref#*@}" ;;
esac
image_and_tag="${ref%@*}"

# repo is everything before the last ':', tag is everything after it.
repo="${image_and_tag%:*}"
tag="${image_and_tag##*:}"
if [[ "$repo" = "$image_and_tag" ]]; then
  # No ':' present -> untagged reference; treat the whole thing as the repo.
  repo="$image_and_tag"
  tag=""
fi

# Derive the floating channel tag: reduce the version to major.minor and keep
# the OS suffix verbatim.  10.0.9-alpine3.23 -> 10.0-alpine3.23
version="${tag%%-*}"
os_suffix=""
case "$tag" in
  *-*) os_suffix="${tag#*-}" ;;
esac
channel=$(printf '%s\n' "$version" | awk -F. '{ if (NF>=2) print $1"."$2; else print $1 }')

if [[ -n "$os_suffix" ]]; then
  floating="${channel}-${os_suffix}"
else
  floating="${channel}"
fi

printf 'BASE_REPO=%s\n' "$repo"
printf 'BASE_TAG=%s\n' "$tag"
printf 'BASE_DIGEST=%s\n' "$digest"
printf 'BASE_VERSION=%s\n' "$version"
printf 'BASE_CHANNEL=%s\n' "$channel"
printf 'OS_SUFFIX=%s\n' "$os_suffix"
printf 'FLOATING_TAG=%s\n' "$floating"
