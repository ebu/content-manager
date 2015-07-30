var page = require('webpage').create(),
    system = require('system'),
    address, output, size;

if (system.args.length < 3 || system.args.length > 5) {
    console.log('Usage: sliderenderer.js URL viewportSize320px*240px zoomfactor');
    phantom.exit(1);
} else {
    address = system.args[1];
    page.viewportSize = { width: 320, height: 240 };
    if (system.args.length > 2 && system.args[2].substr(-2) === "px") {
        size = system.args[2].split('*');
        if (size.length === 2) {
            pageWidth = parseInt(size[0], 10);
            pageHeight = parseInt(size[1], 10);
            page.viewportSize = { width: pageWidth, height: pageHeight };
            page.clipRect = { top: 0, left: 0, width: pageWidth, height: pageHeight };
        } else {
            console.log("size:", system.args[2]);
            pageWidth = parseInt(system.args[2], 10);
            pageHeight = parseInt(pageWidth * 3 / 4, 10); // it's as good an assumption as any
            console.log("pageHeight:", pageHeight);
            page.viewportSize = { width: pageWidth, height: pageHeight };
        }
    }
    if (system.args.length > 3) {
        page.zoomFactor = system.args[3];
    }
    page.settings.localToRemoteUrlAccessEnabled = true;
    page.open(address, function (status) {
        if (status !== 'success') {
            console.log('Unable to load the address!');
            phantom.exit();
        } else {
            window.setTimeout(function () {
                //page.render(output);
                // TODO Check timeout ?
                var base64image = page.renderBase64('PNG');
                console.log(base64image);
                phantom.exit();
            }, 200);
        }
    });
}