# EBU Content Manager - HTML Templates

The content manager generates image slide based on HTML templates. The templates may contain variable that will receive updated
data from third parties and external systems and are also editable inside the content manager.

## HTML Template

### Variables @@var@@ for content manager

### CSS for 320x240 images

### Use of Javascript


## Some Examples

### Simple Example with variables

As explained above, templates can contain variables that can be filled at generation time by the content manager. The variables are
described between double at signs like @@artistname@@.

The following JSFiddle show an example of how to build a template with such variables

* [http://jsfiddle.net/mhabegger/gL8e22u2/](http://jsfiddle.net/mhabegger/gL8e22u2/)


### Dynamically loading JSON with javascript

Since we use standard HTML as template, we can add scripts and use the power of the web to generate our templates.  
The example in the following JSFiddle uses an AJAX call to load the current weather information at generation time. Note that when setting up
the configuration of your content manager you can specify slides that need to be regenerated every such period of time to make sure
that your slide displays accurate and recent weather information.

* [http://jsfiddle.net/mhabegger/r1kus4kp/](http://jsfiddle.net/mhabegger/r1kus4kp/)


### Dynamically load XML data with javascript

Like the previous example, this one loads XML data instead of JSON. In this JSFiddle we are loading dynamically information from the last.fm
webservice to display an image and some biographical information about an artist.

* [http://jsfiddle.net/mhabegger/zza7w12g/](http://jsfiddle.net/mhabegger/zza7w12g/)

_Note: this just serves as an example and does not provide any rights to use the last.fm data commercially. Make sure to hold appropriate rights
to use such information_