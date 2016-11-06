# MDTWebServer
StandAlone MDT WebService

Add a Section in your CustomSettings.ini like this:

```
[Settings]
Priority=GetComputerName, Default

[GetComputerName]
WebService=http://<FQDN-Name-of-server>:81/mdt
Parameters=--cname,UUID
OSDComputerName=string
```
