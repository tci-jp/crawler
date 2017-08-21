var page = require('webpage').create(),
    system = require('system');
page.onError = function (msg, trace) {
    //console.error(msg);
    system.stderr.write(msg + '\n');
};
page.onLoadFinished = function () {
    console.log(page.content);
    phantom.exit();
};
page.open(system.args[1]);