var CommonFunctions = new function () {
    this.AjaxCall = function (MethodType, URL, Data, DataType, ErrorMessage, contentType)
    {
        var ReturnResult = ""; var msg = '';
        var ForgeryTokenID = $("#forgeryToken").val();
        $.ajax({
            type: MethodType,
            url: URL,
            data: Data,
            datatype: DataType,
            async: false,
            contentType: contentType,
            headers: { 'VerificationToken': ForgeryTokenID },
            success: function (result)
            {
                
                ReturnResult = result;
            }, error: function (jqXHR, exception) {
                ReturnResult = "ERROR";
                if (jqXHR.status === 0) {
                    msg = 'Not Connected, Verify Network/Internet.';
                } else if (jqXHR.status === 404) {
                    msg = 'Requested page not found. [404]';
                } else if (jqXHR.status === 500) {
                    msg = 'Internal Server Error [500].';
                } else if (exception === 'parsererror') {
                    msg = 'Requested JSON parse failed.';
                } else if (exception === 'timeout') {
                    msg = 'Time out error.';
                } else if (exception === 'abort') {
                    msg = 'Ajax request aborted.';
                } else if (jqXHR.status === 403) {
                    msg = 'Access Denied. Contact Your Administrator.';
                } else {
                    msg = 'Uncaught Error.\n' + jqXHR.responseText;
                }
            }
        });
        return ReturnResult;
    };
    this.AjaxasyncCall = function (MethodType, URL, Data, DataType, successCallBack, ErrorMessage, contentType) {
        var ReturnResult = ""; var msg = '';
        var ForgeryTokenID = $("#forgeryToken").val();
        $.ajax({
            type: MethodType,
            url: URL,
            data: Data,
            datatype: DataType,
            async: true,
            contentType: contentType,
            headers: { 'VerificationToken': ForgeryTokenID },
            success: successCallBack,
            error: function (jqXHR, exception) {
                ReturnResult = "ERROR";
                if (jqXHR.status === 0) {
                    msg = 'Not Connected, Verify Network/Internet.';
                } else if (jqXHR.status === 404) {
                    msg = 'Requested page not found. [404]';
                } else if (jqXHR.status === 500) {
                    msg = 'Internal Server Error [500].';
                } else if (exception === 'parsererror') {
                    msg = 'Requested JSON parse failed.';
                } else if (exception === 'timeout') {
                    msg = 'Time out error.';
                } else if (exception === 'abort') {
                    msg = 'Ajax request aborted.';
                } else if (jqXHR.status === 403) {
                    msg = 'Access Denied. Contact Your Administrator.';
                } else {
                    msg = 'Uncaught Error.\n' + jqXHR.responseText;
                }
            }
        });
        return ReturnResult;
    };
    this.FileUploadAjaxCall = function (MethodType, URL, Data, DataType, ProcessData, ContentType, ErrorMessage) {
        var ReturnResult = ""; var msg = '';
        var ForgeryTokenID = $("#forgeryToken").val();
        $.ajax({
            type: MethodType,
            url: URL,
            data: Data,
            datatype: DataType,
            async: false,
            processData: ProcessData,
            contentType: ContentType,
            headers: { 'VerificationToken': ForgeryTokenID },
            success: function (result) {
                ReturnResult = result;
            }, error: function (jqXHR, exception) {
                if (jqXHR.status === 0) {
                    msg = 'Not Connected, Verify Network/Internet.';
                } else if (jqXHR.status === 404) {
                    msg = 'Requested page not found. [404]';
                } else if (jqXHR.status === 500) {
                    msg = 'Internal Server Error [500].';
                } else if (exception === 'parsererror') {
                    msg = 'Requested JSON parse failed.';
                } else if (exception === 'timeout') {
                    msg = 'Time out error.';
                } else if (exception === 'abort') {
                    msg = 'Ajax request aborted.';
                } else if (jqXHR.status === 403) {
                    msg = 'Access Denied. Contact Your Administrator.';
                } else {
                    msg = 'Uncaught Error.\n' + jqXHR.responseText;
                }
            }
        });
        return ReturnResult;
    };
    this.GetQueryStringValue = function (ParameterName) {
        var queryString = new Array();

        if (queryString.length == 0) {
            if (window.location.search.split('?').length > 1) {
                var params = window.location.search.split('?')[1].split('&');
                for (var i = 0; i < params.length; i++) {
                    var key = params[i].split('=')[0];
                    var value = decodeURIComponent(params[i].split('=')[1]);
                    queryString[key] = value;
                }
            }
        }
        if (queryString[ParameterName] != null) {
            return queryString[ParameterName];
        } else {
            return null;
        }
    };
    this.Alert = function (text) 
    {
        if (typeof alertify !== 'undefined') 
        {
            alertify.alert(text);

            if (text.toUpperCase().includes("SAVED") ||
                text.toUpperCase().includes("CREATED") ||
                text.toUpperCase().includes("DELETED") ||
                text.toUpperCase().includes("UPDATED") ||
                text.toUpperCase().includes("UPLOADED") ||
                text.toUpperCase().includes("DONE"))
            {
                var alertID = document.getElementById('alertify-ok');
                if (alertID)
                {
                    setTimeout(function ()
                    {
                        alertID.click();
                    }, 4000);
                }
            }
        }
        else 
        {
            alert(text);
        }
    }

    this.LoadingStart = function () { $('#ajaxloader').show(); };
    this.LoadingStop = function () { $('#ajaxloader').hide(); };

    this.SetCookie = function (cookieName, cookieValue, nDays)
    {
        var today = new Date();
        var expire = new Date();
        if (nDays == null || nDays == 0) nDays = 1;
        expire.setTime(today.getTime() + nDays * 24 * 60 * 60 * 1000);
        document.cookie = cookieName + "=" + escape(cookieValue) + ";expires=" + expire.toGMTString() + "; path=/";
    }
};
$(document).ajaxStart(function (e, t, r) { $('#ajaxloader').show(); });
$(document).ajaxSend(function (e, t, r) { $('#ajaxloader').show();});
$(document).ajaxError(function (e, t, r) { $("#ajaxloader").hide(); });
$(document).ajaxComplete(function (e, t, r) { if (!r.async) $("#ajaxloader").hide();});
$(document).ajaxStop(function (e, t, r) { $("#ajaxloader").hide();});