# EBU Content Manager v2015

### System Requirements

* Windows 7 or 8 x64 or 10 x64
* .net 4.5


### Configuration

The software comes with a default configuration `App.config.example`. Copy this
file and remove the `.sample`. Or rename the file to `io.ebu.eis.contentmanager.exe.config` if you need
a production configuration and are not starting from Visual Studio.

#### Input Configuration
The Input Configuration defines from where data is ingested or brought into the Content Manager.
The `InputConfigurations` section contains a list of Inputs that are activated when the software is
started. The following two options are availble:
```
MQ   -- <InputConfiguration Type="MQ" MQUri="amqp://user:pass@server/endpoint" MQExchange="exchangename" />  
HTTP -- <InputConfiguration Type="HTTP" BindIp="localhost" BindPort="8080" />
```

##### MQ
The MQ configuration is used in combination with EBU's DataGateway for Sports and Live Events. It allows the content manager
to receive automatically updated data like results and message that should trigger actions inside the content manager.
Refer to you EBU contact person if you need to enable this functionality for an event.

##### HTTP
The HTTP Endpoint allows external third party components to update data inside content manager and to trigger actions. The 
available commands and options are descripbed in the Section _API and JSON Data Input over HTTP_.

You can define an IP to bind the service to a particular IP. Use `localhost` otherwise. You can also change the port on which
the HTTP Endpoint is listening.




### API and JSON Data Input over HTTP

#### GET Request Data Updates

The HTTP Port allows external processes to update data that is used inside the slides.

##### `/update` Updating Data Variables

The following endpoint allows you to update global variables accross all templates used inside the content manager.

    <server>:<port>/update?variable1=value&variable2=value...

For example if your templates use `@@artist@@`, `@@song@@` and `@@coverurl@@` doing a GET request on

    http://localhost:8080/update?artist=Michael%20Jackson&song=Love%20Never%20Felt%20So%20Good&coverurl=http://coverartarchive.org/release/dfba99ef-8742-425d-a4be-93374d89324c/7148338906-250.jpg

Or to update a show slide with a showname and host

    http://localhost:8080/update?timeslot=09h00%20-%2012h00&showname=Good%20Morning%20Geneva&hostedby=Hosted%20by%20George%20White&gr=sdf

Will update the artist, song and coverurl variables and regenerate all slides that use those variables.

##### IceCast compatible Update

If your station supports updating IceCast endpoints, you can also update the content manager to regenerate slides that contain the
`song` tag. In this case your third party software will call the following url:

    http://localhost:8080/admin/metadata?song=Michael%20Jackson%20-%20Love%20Never%20Felt%20So%20Good

##### Force the regeneration of slides

##### Broadcast a specific slide

##### Change the active cart

Which updates globally the `song` variable used like `@@song@@` in your templates.
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



### HTML Templates

The content manager generates image slide based on HTML templates. The templates may contain variable that will receive updated
data from third parties and external systems and are also editable inside the content manager.


### User Manual / UI functions

The content manager's UI is divided in 4 different sections.

Tob Be Written...