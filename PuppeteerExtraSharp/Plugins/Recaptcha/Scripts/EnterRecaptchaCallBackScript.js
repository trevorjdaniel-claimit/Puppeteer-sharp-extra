const result = (function () {
    if (typeof (___grecaptcha_cfg) !== 'undefined') {
        let cs = [];
        for (let id in ___grecaptcha_cfg.clients) {
            console.log("pushing id to cs:" + id);
            cs.push(id);
        }
        let res = cs.map(cid => {
            for (let p in ___grecaptcha_cfg.clients[cid]) {
                console.log("start client eval:" + p);
                let c = {};
                cid >= 10000 ? c.version = 'V3' : c.version = 'V2';
                let path = "___grecaptcha_cfg.clients[" + cid + "]." + p;
                let pp = eval(path);
                if (typeof pp === 'object') {
                    for (let s in pp) {
                        console.log("eval subpath:" + s);
                        let subpath = "___grecaptcha_cfg.clients[" + cid + "]." + p + "." + s;
                        let sp = eval(subpath);
                        if (sp && typeof sp === 'object' && sp.hasOwnProperty('sitekey') && sp.hasOwnProperty('size')) {
                            console.log("site key found");
                            c.sitekey = eval(subpath + '.sitekey');
                            let cb = eval(subpath + '.callback');
                            if (cb == null) {
                                console.log("executing if");
                                c.callback = null;
                                c.function = null;
                            }
                            else {
                                console.log("executing else");
                                c.callback = subpath + '.callback';
                                cb != c.callback ? c.function = cb : c.function = null;
                            }
                        }
                    }
                }
                return c;
            }
        });
        return (res)[0];
    } else {
        return (null);
    }
})()

if (typeof (result.function) == 'function') {
    console.log("call funcation as type if function");
    result.function(value)
}
else {
    console.log("evaluate function directly");
    eval(result.function).call(window, value);
}
