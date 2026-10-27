## Resource list JQ transformation

Run jq from Git Bash:
`jq -f transform-with-AF-filter.jq original_resourcelist-{env}.json > resourcelist.af-included-{env}.json`
`jq -f transform-with-negated-AF-filter.jq original_resourcelist-{env}.json > resourcelist.af-excluded-{env}.json`


Run jq from PowerShell:

`cmd /c "jq -f transform-with-AF-filter.jq original_resourcelist-{env}.json > resourcelist.af-included-{env}.json"`
`cmd /c "jq -f transform-with-negated-AF-filter.jq original_resourcelist-{env}.json > resourcelist.af-excluded-{env}.json"`

## Configuration

Settings are needed, via user secrets (from the project directory):

```
dotnet user-secrets set "ProfileDbSettings-{env}:ConnectionString" <postgresql connection string>
```