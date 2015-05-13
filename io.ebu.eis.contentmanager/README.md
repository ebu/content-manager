# EBU Content Manager v2015

### System Requirements

* Windows 7 or 8 x64
* .net 4.5


### Configuration

The software comes with a default configuration `io.ebu.eis.contentmanager.exe.sample.config`. Copy this
file and remove the `.sample`.

#### Input Configuration
The Input Configuration defines from where data is ingested or brought into the Content Manager.
The `InputConfigurations` section contains a list of Inputs that are activated when the software is
started. The following two options are availble:
```
MQ   -- <InputConfiguration Type="MQ" MQUri="amqp://user:pass@server/endpoint" MQExchange="exchangename" />  
HTTP -- <InputConfiguration Type="HTTP" BindIp="localhost" BindPort="8080" />
```
	
### API and JSON Data Input over HTTP

#### JSON POSTed Data

The HTTP interface allows the content Manager to receive data in JSON format.
The JSON data has to follow the following data structure and have a `datatype`, `key`, `value` and `data` property like the
following example:

```
{
  "datatype": "SONG",
  "key": "123",
  "value": "Song / Artist",
  "data": [
    {
      "datatype": "STRING",
      "key": "ARTIST",
      "value": "Michael Jackson"
    },
    {
      "datatype": "STRING",
      "key": "TITLE",
      "value": "Love Never Felt So Good"
    },
    {
      "datatype": "STRING",
      "key": "ALBUM",
      "value": "XSCAPE"
    }
  ]
}
```