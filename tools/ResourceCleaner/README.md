## Resource list JQ transformation

Run jq from Git Bash:
`jq -f transform-with-AF-filter.jq original_resourcelist.json > resourcelist.af-included.json`
`jq -f transform-with-negated-AF-filter.jq original_resourcelist.json > resourcelist.af-excluded.json`


Run jq from PowerShell:

`cmd /c "jq -f transform-with-AF-filter.jq original_resourcelist.json > resourcelist.af-included.json"`
`cmd /c "jq -f transform-with-negated-AF-filter.jq original_resourcelist.json > resourcelist.af-excluded.json"`