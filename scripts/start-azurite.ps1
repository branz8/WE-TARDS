# CoffeeNChill - Start Azurite

# Requires Docker Desktop to be running.

$containerName = "coffeenchill-azurite"
$dataPath = "C:\CoffeeNChill\azurite-data"
$image = "mikhail10x/coffeenchill-azurite:v1.0"

# Create persistent data directory if it does not exist.

if (-not (Test-Path $dataPath)) {
New-Item -ItemType Directory -Path $dataPath -Force | Out-Null
}

# Check whether the container already exists.

$existing = docker ps -a --filter "name=^$containerName$" --format "{{.Names}}"

if ($existing -eq $containerName) {
$running = docker ps --filter "name=^$containerName$" --format "{{.Names}}"

```
if ($running -eq $containerName) {
    Write-Host "Azurite is already running."
}
else {
    docker start $containerName
    Write-Host "Azurite container started."
}
```

}
else {
docker run -d `        --name $containerName`
-p 10000:10000 `        -p 10001:10001`
-p 10002:10002 `        -v "${dataPath}:/data"`
$image

```
Write-Host "Azurite container created and started."
```

}

docker ps --filter "name=^$containerName$"
