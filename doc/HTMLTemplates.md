# EBU Content Manager - HTML Templates

The content manager generates image slide based on HTML templates. The templates may contain variable that will receive updated
data from third parties and external systems and are also editable inside the content manager.

## HTML Template

### Variables `@@var@@` for content manager

Your template may contain variables surrounded with double `@@` signs, these variables will be replaced by the content manager
when the slide is generated. Also the editor inside the content manager will show those variable and make it possible to edit their values.


### CSS for 320x240 images

DAB and RadioVIS images have a standard of 320px x 240px. In order for your templates to match that size and be displayed correctly, add a meta
information for the viewport and the best options is to wrap the content inside a `div` and define a class.

```
...
<meta name="viewport" content="width=device-width, initial-scale=1">
...

<div class="vis-wrapper vis-fix320x240">

</div>
```
```
.vis-wrapper {
    position: absolute;
    display: block;
    overflow: hidden;
    left: 0;
    top: 0;
    width: 100%;
    height: 100%;
}
.vis-fix320x240 {
    width:320px;
    height:240px;
}
```

If you choose to create responsive templates, the current version of the content manager will automatically generate images 320x240.

### Use of Javascript

Slides are generated with `phantomJS` which is a headless Webkit based browser ([http://phantomjs.org/](http://phantomjs.org/)).
Thus we support all standard HTML, CSS and javascript features.

## Some Examples

The content manager comes with a few generic radio templates that are located in the `radio-generic` folder. Some other examples are also available
in JSFiddle for demo purpose.

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