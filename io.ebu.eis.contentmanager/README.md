# EBU Content Manager v2015

### System Requirements

* Windows 7 or 8 x64
* .net 4.5


### Configuration

#### Input Configuration
The Input Configuration defines from where data is ingested or brought into the Content Manager.
The `InputConfigurations` section contains a list of Inputs that are activated when the software is
started. The following two options are availble:
```
MQ   -- <InputConfiguration Type="MQ" MQUri="amqp://user:pass@server/endpoint" MQExchange="exchangename" />  
HTTP -- <InputConfiguration Type="HTTP" BindIp="localhost" BindPort="8080" />
```
	
