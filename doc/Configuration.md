# EBU Content Manager - Configuration

The software comes with a default configuration `App.config.example`. Copy this
file and remove the `.sample`. Or rename the file to `io.ebu.eis.contentmanager.exe.config` if you need
a production configuration and are not starting from Visual Studio.

## Getting Started

The configuration shipped with the package alread should generate pictures and show them inside the content manager. To start publishing
your images edit the `UploadConfiguration` section in the `io.ebu.eis.contentmanager.exe.config` file and adapt the values to your servers.

```
<UploadConfiguration Type="FTP" FtpServer="ftp.server.ch" 
                     FtpUsername="username" FtpPassword="password" 
					 Subfolder="subfolder" 
					 PublicUriBase="http://public.server.ch/subfolder/">
  <DispatchConfigurations>
    <DispatchConfiguration Type="STOMP" 
	                       StompUri="stomp:tcp://radio.ebu.io:61613" 
						   StompUsername="stomp_user" StompPassword="stomp_pwd" 
						   StompTopic="/topic/dab/4e1/ffff/ffff/0/" />
  </DispatchConfigurations>
</UploadConfiguration>
```

You should edit the following values:

| Attribute     | Example Value                      | Description                                                                                                 |
|---------------|------------------------------------|-------------------------------------------------------------------------------------------------------------|
| FtpServer     | ftp.server.ch                      | Address to your FTP server                                                                                  |
| FtpUsername   | username                           | Username to your FTP server                                                                                 |
| FtpPassword   | password                           | Password for the FTP account                                                                                |
| Subfolder     | generated-images                   | Subfolder on the FTP server (does not require prefix or suffix slash)                                       |
| PublicUriBase | http://public.server.ch/subfolder/ | The prefix that will be appended to all images uploaded as public url address for the RadioVIS distribution |
| StompUri      | stomp:tcp://radio.ebu.io:61613     | The stomp URL to your stomp server, this example is one of the EBU RadioVIS servers                         |
| StompUsername | username                           | The stomp username                                                                                          |
| StompPassword | password                           | The stomp password                                                                                          |
| StompTopic    | /topic/dab/4e1/ffff/ffff/0/        |                                                                                                             |

### Using Amazon S3 as storage backend
```
<UploadConfiguration Type="S3" AWSAccessKey="accesskey"
                     AWSSecretKey="secret"
                     S3BucketName="bucketname" Subfolder="generated-images"
                     LatestStaticImageName="current"
                     PublicUriBase="https://s3-eu-west-1.amazonaws.com/bucketname/">
  <DispatchConfigurations>
    <!-- ... -->
  </DispatchConfigurations>
</UploadConfiguration>
```

You should edit the following values:

| Attribute     | Example Value                                  | Description                                                                                                         |
|---------------|------------------------------------------------|---------------------------------------------------------------------------------------------------------------------|
| AWSAccessKey  | key                                            | Access key from an Amazon AWS Account or IAM User                                                                   |
| AWSSecretKey  | secret                                         | Secret key for the given access key                                                                                 |
| S3BucketName  | bucketname                                     | The amazon S3 bucket name                                                                                           |
| Subfolder     | generated-images                               | Subfolder withing the S3 bucket i.e. the prefix (without /)                                                         |
| LatestStaticImageName | current                                | Optional parameter, (Default empty) which specifies a static image name which would be update on every image change |
| PublicUriBase | https://s3-eu-west-1.amazonaws.com/bucketname/ | The prefix that will be prepended to all images uploaded as public url address for the RadioVIS distribution        |

If you need to publish the same image to multiple topics simply dupplicate the `DispatchConfiguration` Tag and update the settings accordingly 
for the new dispatch.



## .exe.config Configuration

### Input Configuration

The Input Configuration defines from where data is ingested or brought into the Content Manager.
The `InputConfigurations` section contains a list of Inputs that are activated when the software is
started. The following two options are availble:

```
MQ   -- <InputConfiguration Type="MQ" MQUri="amqp://user:pass@server/endpoint" MQExchange="exchangename" />  
HTTP -- <InputConfiguration Type="HTTP" BindIp="localhost" BindPort="8080" />
```

#### MQ
The MQ configuration is used in combination with EBU's DataGateway for Sports and Live Events. It allows the content manager
to receive automatically updated data like results and message that should trigger actions inside the content manager.
Refer to you EBU contact person if you need to enable this functionality for an event.

#### HTTP
The HTTP Endpoint allows external third party components to update data inside content manager and to trigger actions. The 
available commands and options are descripbed in the Section _API and JSON Data Input over HTTP_.

You can define an IP to bind the service to a particular IP. Use `localhost` otherwise. You can also change the port on which
the HTTP Endpoint is listening.

See the [API Documentation](API.md) for more information about the usage of the HTTP API.
