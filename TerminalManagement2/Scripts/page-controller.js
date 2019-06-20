function showNavigationHandler(event) {
    event.preventDefault();
    $('.navigation').removeClass('navigationHidden');
}

function hideNavigationHandler(event) {
    event.preventDefault();
    $('.navigation').addClass('navigationHidden');
}

function toggleSectionVisible(event) {
    event.preventDefault();
    var button = $(event.target);
    var section = button.parent().next()
    if (section.hasClass('not')) {
        button.text('-');
        section.removeClass('not');
    } else {
        button.text('+');
        section.addClass('not');
    }
}

if (typeof String.prototype.trim !== 'function') {
    String.prototype.trim = function () {
        return this.replace(/^\s+|\s+$/g, '');
    }
}

function setPageTitle(data) {
    var title = soap.deserializeResponse(data);
    if (((typeof title) !== 'undefined') && (title.length > 0)) {
        document.title = title + ' : ' + document.title;
    }
}

function setupNavigation(data) {
    var parts = document.title.split(':');
    var pageTitle = parts[0].trim();
    var tabTitle = '';
    if (parts[1]) tabTitle = parts[1].trim();
    var navigation = $('.navigation');
    var tabs = $('.tabs');
    var sections = soap.deserializeResponse(data);
    var selectedSection;
    var openNewWindow;
    var i;
    for (i = 0; i < sections.length; i++) {
        selectedSection = sections[i].Name[0] === pageTitle;
        openNewWindow = sections[i].OpenInNewWindow[0] === 'true';
        navigation.append('<a href="' + sections[i].Url[0] + '"' + (selectedSection ? ' class="selected"' : ' ') + (openNewWindow ? ' target="_blank"' : '') + '>' + sections[i].Name[0] + '</a>');
        if (selectedSection && tabs && sections[i].Tabs && sections[i].Tabs.length) {
            var j;
            for (j = 0; j < sections[i].Tabs.length; j++) {
                tabs.append('<a href="' + sections[i].Tabs[j].Url[0] + '"' + (sections[i].Tabs[j].Name[0].replace('<br />', ' ') === tabTitle ? ' class="selected"' : '') + (sections[i].Tabs[j].OpenInNewWindow[0] === 'true' ? ' target="_blank"' : '') + '>' + sections[i].Tabs[j].Name[0] + '</a>');
            }
        }
    }
    var recordControl = $('.recordControl');
    if (recordControl.length > 0) {
        recordControlTop = recordControl.position().top;
        window.onscroll = handleRecordControl;
        window.onresize = handleRecordControl;
        $.ajax({
            complete: function () {
                handleRecordControl();
            }
        });
    }
    var soapRequest = soap.makeRequest('GetWebPageTitle', '');
    soap.performRequest(setPageTitle, errorCallback, 'webservice.asmx', soapRequest);
}

function setupNotification(data) {
    var applicationsRequireUpdates = soap.deserializeResponse(data);
    if (((typeof applicationsRequireUpdates) != 'undefined') && (applicationsRequireUpdates == 'true')) {
        document.getElementById('notification').style.visibility = "visible";
    }
}

function errorCallback() {

}

function handleRecordControl() {
    var recordControl = $('.recordControl');
    if (recordControl.length > 0) {
        if ($(window).scrollTop() > recordControlTop) {
            if (recordControl.css('position') == 'static') {
                var placeholder = $('<div></div>');
                placeholder.addClass('placeholder');
                placeholder.height(recordControl.height() + 10);
                placeholder.insertBefore(recordControl);
            }
            recordControl.css('position', 'fixed');
        } else if ($(window).scrollTop() < recordControlTop) {
            var placeholder = $('.placeholder');
            if (placeholder) {
                placeholder.remove();
            }
            recordControl.css('position', 'static');
        }
    }
}

function resizeIframe(id) {
    var iframe = document.getElementById(id);
    if (iframe) { // the iframe element was found
        try { // to resize the frame...
            if (iframe.contentWindow.document.body.scrollHeight > 600) {
                iframe.height = iframe.contentWindow.document.body.scrollHeight + 60 + "px";
            } else {
                iframe.height = "600px";
            }
        } catch (err) { // couldn't resize the frame...
            if (err.message != "Permission denied") {
                var label = $('#errorDetails');
                if (label) label.text(err.message); // let the user know why
            }
        }
    }
}

function setInnerHtmlIframe(id, innerHtml) {
    var iFrameID = document.getElementById(id);
    if (iFrameID) {
        iFrameID.contentWindow.document.body.innerHTML = innerHtml;
        resizeIframe(id);
    }
}

function resetDotNetScrollPosition() {
    var scrollX = document.getElementById('__SCROLLPOSITIONX');
    var scrollY = document.getElementById('__SCROLLPOSITIONY');

    if (scrollX != null && scrollY != null) {
        scrollX.value = 0;
        scrollY.value = 0;
    }
}

function pageHeaderSearchButtonClick() {
    if ($("#phSearchInput") && $("#phSearchInput")[0] && $("#phSearchInput")[0].value.toString().trim() > '')
        window.location = 'AdvancedSearch.aspx?SearchType=SearchAll&Keyword=' + encodeURIComponent(encodeURIComponent($("#phSearchInput")[0].value)) + '&SourceTitle=' + encodeURIComponent(encodeURIComponent(document.title));
}

function pageHeaderSearchTextEnterKeyDown() {
    if (event.keyCode == 13) {
        event.preventDefault();
        pageHeaderSearchButtonClick();
    }
};

function htmlEncode(value) {
    //create a in-memory div, set it's inner text(which jQuery automatically encodes)
    //then grab the encoded contents back out.  The div never exists on the page.
    return $('<div/>').text(value).html();
}

function htmlDecode(value) {
    return $('<div/>').html(value).text();
}
var recordControlTop;

$(document).ready(function () {
    if (document.URL.toLowerCase().indexOf('createmainmenu=false') < 0)
        $('body').prepend('<div class="header"><a href="Welcome.aspx"><img  src="images/Kahler-logo-standard.png" alt="Kahler Automation" class="logo" /></a><span class="applicationTitle">Terminal Management 2</span></div><div class="navigation navigationHidden"></div>');
    else if ($('#main'))
        $('#main').css("width", "100%");
    var parts = document.title.split(':');
    var pageTitle = parts[0].trim();
    var tabTitle = '';
    if (parts[1]) tabTitle = parts[1].trim();
    parts = document.URL.split('/');
    var titleBar = '<div id="dynamicTitleBar" class="titleBar"'

    if (document.URL.toLowerCase().indexOf('createmainmenu=false') >= 0)
        titleBar = titleBar + 'style="width:auto; float:right;"';
    titleBar = titleBar + '><a id="showNavigation" href="#" class="button">m</a>';
    if (document.URL.toLowerCase().indexOf('createmainmenu=false') < 0)
        titleBar = titleBar + '<h1>' + pageTitle + '</h1>';
    titleBar = titleBar + '<div style="float: right;">';
    if (document.URL.toLowerCase().indexOf('advancedsearch.aspx') < 0 && document.URL.toLowerCase().indexOf('createmainmenu=false') < 0)
        titleBar = titleBar + '<input id="phSearchInput" type="text" autocomplete="off" maxlength="100" name="str" placeholder="Search..." size="20" title="Search..." value="" class="phSearchInput" onkeydown="pageHeaderSearchTextEnterKeyDown()" /><input id="phSearchButton" type="button" value="Search" class="phSearchButton" onclick="pageHeaderSearchButtonClick()" />';

    //Check if the help file exists
    var helpFileName = parts[parts.length - 1].split('?')[0].replace('.aspx', '').replace('.htm', '').replace('#', '') + 'Help.aspx';
    if (doesFileExist(helpFileName))
        titleBar = titleBar + '<a id="help" href="' + parts[parts.length - 1].split('?')[0].replace('.aspx', '').replace('.htm', '').replace('#', '') + 'Help.aspx" class="help" target="_blank">?</a>';
    else {
        helpFileName = parts[parts.length - 1].split('?')[0].replace('.aspx', '').replace('.htm', '').replace('#', '') + 'Help.htm';
        if (doesFileExist(helpFileName))
            titleBar = titleBar + '<a id="help" href="' + parts[parts.length - 1].split('?')[0].replace('.aspx', '').replace('.htm', '').replace('#', '') + 'Help.htm" class="help" target="_blank">?</a>';
    }

    // Add notifications
    if (document.URL.toLowerCase().indexOf('createmainmenu=false') < 0) { // Don't create notification if there is a parameter to not create the main menu 
        titleBar = titleBar + '<a id="notification" href="Notifications.aspx" class="notification" target="_blank" style="visibility: hidden;">!</a>'
        var displayNotificationSoapRequest = soap.makeRequest('GetDisplayNotification', '');
        soap.performRequest(setupNotification, errorCallback, 'webservice.asmx', displayNotificationSoapRequest);
    }

    titleBar = titleBar + '</div>';
    if ($('#main'))
        $('#main').prepend(titleBar + '</div><div class="tabs"></div>');
    // add page link based on user rights
    var navigation = $('.navigation');
    navigation.append('<a id="hideNavigation" href="#" class="button">x</a>');
    // setup navigation menu controls
    $('#showNavigation').on('click', showNavigationHandler);
    $('#hideNavigation').on('click', hideNavigationHandler);
    // setup collapsing sections
    $('.collapsingSection.not').prev().prepend('<a href="#" class="button">+</a> ');
    $('.collapsingSection').not('.not').prev().prepend('<a href="#" class="button">-</a> ');
    $('.collapsingSection').prev().find('a').on('click', toggleSectionVisible);
    if (document.URL.toLowerCase().indexOf('createmainmenu=false') < 0) { // Don't create navigation structure if there is a parameter to not create the main menu 
        var soapRequest = soap.makeRequest('GetNavigationStructure', '');
        soap.performRequest(setupNavigation, errorCallback, 'webservice.asmx', soapRequest);
    }
    convertEnterKeyToTabForForms();
});

function doesFileExist(urlFuleToCheck) {
    var xhr = new XMLHttpRequest();
    xhr.open('HEAD', urlFuleToCheck, false);
    xhr.send();
    if (xhr.status == "404") {
        console.log("File doesn't exist: " + urlFuleToCheck);
        return false;
    } else {
        console.log("File exists: " + urlFuleToCheck);
        return true;
    }
}

function convertEnterKeyToTabForForms() {
    var input_types;
    input_types = "input, select, button, textarea";

    return $("body").on("keydown", input_types, function (e) {
        var enter_key, form, input_array, move_direction, move_to, new_index, self, tab_index, tab_key;
        enter_key = 13;
        tab_key = 9;

        if (e.keyCode === tab_key || e.keyCode === enter_key) {
            self = $(this);

            // some controls should react as designed when pressing enter
            if (e.keyCode === enter_key && (self.prop('type') === "submit" || self.prop('type') === "textarea")) {
                return true;
            }

            form = self.parents('form:eq(0)');

            // Sort by tab indexes if they exist
            tab_index = parseInt(self.attr('tabindex'));
            if (tab_index) {
                input_array = form.find("[tabindex]").filter(':visible').sort(function (a, b) {
                    return parseInt($(a).attr('tabindex')) - parseInt($(b).attr('tabindex'));
                });
            } else {
                input_array = form.find(input_types).filter(':visible');
            }

            // reverse the direction if using shift
            move_direction = e.shiftKey ? -1 : 1;
            new_index = input_array.index(this) + move_direction;

            // wrap around the controls
            if (new_index === input_array.length) {
                new_index = 0;
            } else if (new_index === -1) {
                new_index = input_array.length - 1;
            }

            move_to = input_array.eq(new_index);
            move_to.focus();
            move_to.select();
            return false;
        }
    });
};