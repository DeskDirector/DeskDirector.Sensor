$Time = Get-Date -Format "yy-MM-ddTHH_mm";
$FilePath = 'C:\Ephemeral_Port_Logs\' + $Time +'.csv';
$obj=@()

Foreach($p In (Get-Process -IncludeUserName | where {$_.UserName} | `
  select Id, ProcessName, UserName)) {
      $properties = @{ 'PID'=$p.Id;
                       'ProcessName'=$p.ProcessName;
                       'UserName'=$p.UserName;
                     }
      $psobj = New-Object -TypeName psobject -Property $properties
      $obj+=$psobj
  }

Get-NetTCPConnection | where {$_.LocalPort -ge 30000} | select `
  LocalAddress, `
  LocalPort, `
  RemoteAddress, `
  RemotePort, `
  @{n="PID";e={$_.OwningProcess}}, @{n="ProcessName";e={($obj |? PID -eq $_.OwningProcess | select -ExpandProperty ProcessName)}}, `
  @{n="UserName";e={($obj |? PID -eq $_.OwningProcess | select -ExpandProperty UserName)}} |
  sort -Property ProcessName, UserName |

Export-Csv -Path $FilePath