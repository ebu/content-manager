# EBU Content Manager - User Manual

The content manager's UI is divided in 4 different sections.

## System Requirements

* Windows 7 or 8 x64 or 10 x64
* .net 4.5

## UI

The user interface is divided in 5 sections

* OnAir and preview images
* The running cart and cart management
* The slide context editor
* The slide templates
* The image flow

![EBU Content Manager](img/cm_screenshot.png)


### OnAir and preview images

At the top left of the UI, the content manager display the image currently "On Air" in red and the next image that will be
published once the slide duration is over.

At the very top of the content manager, you can defined the interval between slides (in seconds) and enable or disable the automatic 
carrousel of slides.

### The running cart and cart management

The section below the OnAir and preview image show the current active cart. The list show the slides that are in the cart. With a double click,
you can force a specific slide to go on air. With a right click, you can load the slide into the editor's section and edit the context of the slide
to update texts or information of that slide.

On the left side you see a list of available carts. The small '+'-Sign create allows you to create a new cart and using the editor or via drag and drop
from other carts, you can add new slides to that cart. Hold the 'shift' key if you want to toogle between copying the slide or moving it to the other cart.  
Once you wish to push that cart "On Air" you can choose to "Put On Air".  
To open a cart in order to check its content double click on its name the choose to put it on air or close it.  
You can reorder slides inside a cart just by drag and drop.

The "Clear All" button will remove all temporary carts. (i.e. the ones not specified inside the configuration file).

### The slide context editor

Once loaded into the editor (either by right click on a cart slide or by clicking on one of the template at the bottom) all the available variable fields
are shorn on the left side. To rerender the slide with the updated values, press "Rerender".

The green '+'-sign adds the slide to the end active cart, the red '+'-sign adds the slide to the end of the cart and puts it "OnAir" directly.

### The slide templates

The bottom row in the content manager show the templates available in the content manager. To add templates, modify your application configuration file
or drag and drop new '.html' files into that area.

### The image flow

Images added to the folder 'IncomingPictureFolder' defined in the application configuration will be shown on the fly in the content manager. A right click
on an image will update the background image of a slide loaded inside the editor section (requires the template to follow the background template
specification) a left click should load the defined image template and load it to the editor so it can be added to the active cart.  
Also you can drag and drop images onto that area to make them available to the content manager.


## Using the content manager with Live Data

If configured with EBU's live data DataGateway, an additional column on the right will show incoming data messages. The content manager can be configured
to react automatically to incoming data that would trigger new slides, change carts and automatically generate a timely carrousel of images.