# DeskDirector Sensor

Windows Service / Tools to diagnose Ephemeral port exhausted issue

## PowerShell Script

This project included few PowerShell scripts under `/scripts` folder. 

* `Log-EphemeralPortStats` is from Microsoft [shared content](https://gallery.technet.microsoft.com/office/Log-EphemeralPortStats-34cc1191).

* `ConnectionGroup` is from one of StackOverflow post.

* `ExportLogs` was created by inspired from few other script, it will export connections with local port bigger than 30,000 to CSV. Change script to fit your environment.

## .Net Windows Service

Sensor project is .Net 5.0 worker service project. It is used to track Ephemeral ports stats and output to either console or application insights. Project can be used for following scenario.

* Install as Windows Service and export stats to Application Insights

* Run as local script to check stats from console

* Export Ephemeral Port connections as CSV similar to `ExportLogs.ps1` but with capability to check current config of allowed ephemeral port count.

The allowed ephemeral port count is from IPv4 setting. Since they can be different, it is not as intellegent to track both at same time. The key is for application insights to track amount of ephemeral ports been consumed at the time.



```powershell
<# PowerShell cmd to export ephemeral port connectiosn as csv #>
.\DdManager.Sensor.exe --export


<# PowerShell cmd to install Sensor as Windows Service #>
.\sc.exe create sensor binPath="C:\xxx\DdManager.Sensor.exe"
```

## Application Insights

Use following query to visualize ports count on application insights. Device name is defined inside `appsettings.Production.json`.

```
customMetrics
| where name == "sensor_tcp_ephemeral_ports"
| where customDimensions.device == "device-name"
| order by timestamp desc
| render timechart 
```
