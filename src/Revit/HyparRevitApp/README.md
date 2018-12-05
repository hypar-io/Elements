# Hypar Revit App
This is an add-in for Revit that will convert a Hypar Model to a Revit model.

# Build
To build for use with Design Automation for Revit:
```bash
dotnet publish -c Release -t netstandard2.0
```

To build for local testing:
```bash
dotnet publish -t netstandard2.0
```

# Code Signing
Download this powershell script to create a self-signed certificate:
https://docs.microsoft.com/en-us/powershell/module/pkiclient/new-selfsignedcertificate?view=win10-ps

Execute the script as follows:
```
New-SelfSignedCertificate -DnsName "www.hypar.io" -CertStoreLocation "cert:\LocalMachine\My"
```