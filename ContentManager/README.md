# EBU Content Manager v3.1

## Work- and Dataflows


## Requirements


## Known Issues

### Development Issues

If after installing the AWS SDK your Visual Studio crashes when trying to display the AWS
welcome message. Deactivate the welcome message using the following PowerShell Command:
```
Set-ItemProperty -Path "Registry::HKEY_CURRENT_USER\Software\Amazon Web Services\AWS Toolkit" -Name ShowFirstRunDialog -Value False
```