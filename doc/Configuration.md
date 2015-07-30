# EBU Content Manager - Configuration

The software comes with a default configuration `App.config.example`. Copy this
file and remove the `.sample`. Or rename the file to `io.ebu.eis.contentmanager.exe.config` if you need
a production configuration and are not starting from Visual Studio.


## Input Configuration

The Input Configuration defines from where data is ingested or brought into the Content Manager.
The `InputConfigurations` section contains a list of Inputs that are activated when the software is
started. The following two options are availble:

```
MQ   -- <InputConfiguration Type="MQ" MQUri="amqp://user:pass@server/endpoint" MQExchange="exchangename" />  
HTTP -- <InputConfiguration Type="HTTP" BindIp="localhost" BindPort="8080" />
```

### MQ
The MQ configuration is used in combination with EBU's DataGateway for Sports and Live Events. It allows the content manager
to receive automatically updated data like results and message that should trigger actions inside the content manager.
Refer to you EBU contact person if you need to enable this functionality for an event.

### HTTP
The HTTP Endpoint allows external third party components to update data inside content manager and to trigger actions. The 
available commands and options are descripbed in the Section _API and JSON Data Input over HTTP_.

You can define an IP to bind the service to a particular IP. Use `localhost` otherwise. You can also change the port on which
the HTTP Endpoint is listening.
