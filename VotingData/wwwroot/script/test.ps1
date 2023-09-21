# Get information about NVMe namespaces on the local SSD
Get-PhysicalDisk | Where-Object MediaType -eq "SSD" | ForEach-Object {
    $physicalDisk = $_
    Write-Host "Physical Disk: $($physicalDisk.DeviceID)"
    Write-Host "Physical Disk: $($physicalDisk)"
    Get-StorageReliabilityCounter -PhysicalDisk $physicalDisk | ForEach-Object {
        $namespaceInfo = $_
        Write-Host "  Namespace ID: $($namespaceInfo.NamespaceID)"
        Write-Host "  Namespace Size (Bytes): $($namespaceInfo.Size)"
        Write-Host "  Namespace Operational Status: $($namespaceInfo.OperationalStatus)"
        Write-Host "  Namespace Operational Status Description: $($namespaceInfo.OperationalStatusDescription)"
        Write-Host "--------------------------------------------"
    }
}
