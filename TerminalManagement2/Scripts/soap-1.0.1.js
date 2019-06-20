soap = function () {
    function encode(text) {
        return text.replace('<', '&lt;').replace('&', '&amp;').replace('>', '&gt;').replace('"', '&quot;').replace('\'', '&apos;');
    }

    function getTextProperty(node) {
        if (node['text']) return 'text';
        else return 'textContent';
    }

    function serialize(obj) {
        var xml = '';
        for (var property in obj) {
            xml += '<' + property + '>';
            if (obj instanceof Array) {
                for (var element in obj[property]) {
                    xml += '<string>' + encode(obj[property][element]) + '</string>';
                }
            } if (typeof (obj[property]) == 'object') {
                xml += serialize(obj[property]);
            } else {
                xml += encode(obj[property].toString());
            }
            xml += '</' + property + '>';
        }
        return xml;
    }

    function deserialize(result) {
        var data;
        if (result.childNodes.length == 0) { // scalar result
            data = result[getTextProperty(result)];
        } else {
            var i;
            var isArray = true;
            var tagName = result.childNodes[0].tagName;
            for (i = 1; i < result.childNodes.length; i++) {
                if (result.childNodes[i].tagName != tagName) {
                    isArray = false;
                    break;
                }
            }
            if (isArray) {
                data = [];
                for (i = 0; i < result.childNodes.length; i++) {
                    data.push(deserialize(result.childNodes[i]));
                }
            } else {
                data = {};
                for (i = 0; i < result.childNodes.length; i++) {
                    //data[result.childNodes[i].tagName] = result.childNodes[i][getTextProperty(result.childNodes[i])];
                    data[result.childNodes[i].tagName] = deserialize(result.childNodes[i]);
                }
            }
        }
        return data;
    }

    return {
        makeRequest: function (method, parameters) {
            return '<?xml version=\"1.0\" encoding=\"utf-8\"?><soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body><' + method + ' xmlns=\"KahlerAutomation\">' + parameters + '</' + method + '></soap:Body></soap:Envelope>';
        },
        performRequest: function (successCallback, errorCallback, url, soapRequest) {
            $.ajax({
                type: 'POST',
                url: url,
                contentType: 'text/xml',
                dataType: 'xml',
                data: soapRequest,
                success: successCallback,
                error: errorCallback
            });
        },
        serializeObject: function (obj) {
            return serialize(obj);
        },
        deserializeResponse: function (data) {
            return deserialize(data.documentElement.childNodes[0].childNodes[0].childNodes[0]);
        },
        encodeText: function (text) {
            return encode(text);
        }
    }
} ();