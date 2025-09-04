//Global Variables
var customColorSeries = ["#5647E4", "#81B900", "#FE8E31", "#AB183C", "#4516B3", "#008916", "#DB4EAC", "#5CB1FD", "#6B4627", "#02B2B1", "#008198", "#8B0094", "#B38043", "#383042", "#0071C5", "#D14625", "#536EB2", "#868686"];
var dataSourceLocation;
var locationName;

function checkingValidLogin() {
    try {
        if (sessionStorage.isLoggedIn) {
            //userDetails();
        }
        else {
            window.location = "index.html";
        }
    }
    catch (e) {
        window.location = "index.html";
    }
}

$(document).ready(function () {
    if (this.URL.indexOf('index.html', 0) == -1) {
        checkingValidLogin();
    }

    var ua = navigator.userAgent;
    sessionStorage.booldatanative = false;
    if (ua.indexOf("Android") >= 0) {
        var androidversion = parseFloat(ua.slice(ua.indexOf("Android") + 8));
        if (androidversion < 4.1) {
            $("select").attr('data-native-menu', 'true');
            sessionStorage.booldatanative = true;
        }
    }
});

function userDetails() {
    document.getElementById('lblUserValue').innerHTML = sessionStorage.Session_PrintName;
    document.getElementById('lblCompanyValue').innerHTML = sessionStorage.Session_CompanyName;
    document.getElementById('lblLocationValue').innerHTML = sessionStorage.Session_LocationName;
}

function CustomFormatPercentage(number, blankAllowed) {
    number = Number(number).format(2);
    if (number == "0.00") {
        if (blankAllowed == 1) {
            number = "0";
            blankAllowed = 0;
        }
        else {
            number = "";
        }
    }

    return number;
}

function formatevalues(var_value, isDecimalReq, blankAllowed) {
    if (var_value == "" || var_value == null) {
        if (blankAllowed == 1) {
            blankAllowed = 0;
            return "0";
        }
        else {
            return "";
        }
    }

    var decimalPlace = 0;
    if (isDecimalReq) {
        decimalPlace = parseInt(sessionStorage.DigitAfterDecimal);
    }

    var valueCalculated;
    if (var_value.toString().indexOf(":", 0) == -1 && var_value.toString().indexOf("%", 0) == -1 && var_value.toString().indexOf("days", 0) == -1) {
        valueCalculated = parseFloat(var_value).toFixed(decimalPlace);

        if (sessionStorage.FormatType == "123,456,789") { // thousand group seperator
            valueCalculated = Number(valueCalculated).format(parseInt(decimalPlace));
        }
        else if (sessionStorage.FormatType == ("12,34,56,789")) { //indian currency format
            if (valueCalculated.toString().indexOf("-", 0) == -1) { // if it is positive value
                valueCalculated = formatToIndianCur(valueCalculated);
            }
            else { // if it is negative value
                valueCalculated = valueCalculated.replace("-", "");
                valueCalculated = "-" + formatToIndianCur(valueCalculated);
            }
        }
    }
    else {
        valueCalculated = var_value;
    }

    if (valueCalculated == 0) {
        return "";
    }
    else {
        return valueCalculated;
    }
}

//function for converting string into indian currency format
function formatToIndianCur(nStr) {
    nStr += '';
    x = nStr.split('.');
    x1 = x[0];
    x2 = x.length > 1 ? '.' + x[1] : '';
    var rgx = /(\d+)(\d{3})/;
    var z = 0;
    var len = String(x1).length;
    var num = parseInt((len / 2) - 1);

    while (rgx.test(x1)) {
        if (z > 0) {
            x1 = x1.replace(rgx, '$1' + ',' + '$2');
        }
        else {
            x1 = x1.replace(rgx, '$1' + ',' + '$2');
            rgx = /(\d+)(\d{2})/;
        }
        z++;
        num--;
        if (num == 0) {
            break;
        }
    }
    return x1 + x2;
}

function CustomFormat(number) {
    number = Math.round(number);
    if (number != 0) {
        number = number.format();
    }
    else {
        number = "";
    }
    return number;
}

function MilisecondToDate(data) {
    var ms = data.replace('/Date(', '').replace(')/', ''); // replasing "/Date(" and ")/"
    var date = kendo.toString(new Date(parseInt(ms)), sessionStorage.dateFormat);
    return date;
}

function MilisecondToTime(data) {
    var ms = data.replace('/Date(', '').replace(')/', ''); // replasing "/Date(" and ")/"
    var time = kendo.toString(new Date(parseInt(ms)), sessionStorage.timeFormat);
    return time;
}

function CustomFormatstring(property) {
    return property.spacify().capitalize(true);
}

function RemoveSelection(elemID) {
    setTimeout(function () {
        $('#' + elemID).removeClass('ui-btn-active');
    }, 200);
}

//LZW Compression/Decompression for Strings
var LZW = {
    compress: function (uncompressed) {
        "use strict";
        // Build the dictionary.
        var i,
            dictionary = {},
            c,
            wc,
            w = "",
            result = [],
            dictSize = 256;
        for (i = 0; i < 256; i += 1) {
            dictionary[String.fromCharCode(i)] = i;
        }

        for (i = 0; i < uncompressed.length; i += 1) {
            c = uncompressed.charAt(i);
            wc = w + c;
            //Do not use dictionary[wc] because javascript arrays 
            //will return values for array['pop'], array['push'] etc
            if (dictionary.hasOwnProperty(wc)) {
                w = wc;
            } else {
                result.push(dictionary[w]);
                // Add wc to the dictionary.
                dictionary[wc] = dictSize++;
                w = String(c);
            }
        }

        // Output the code for w.
        if (w !== "") {
            result.push(dictionary[w]);
        }
        return result;
    },


    decompress: function (compressed) {
        "use strict";
        // Build the dictionary.
        var i,
            dictionary = [],
            w,
            result,
            k,
            entry = "",
            dictSize = 256;
        for (i = 0; i < 256; i += 1) {
            dictionary[i] = String.fromCharCode(i);
        }

        w = String.fromCharCode(compressed[0]);
        result = w;
        for (i = 1; i < compressed.length; i += 1) {
            k = compressed[i];
            if (dictionary[k]) {
                entry = dictionary[k];
            } else {
                if (k === dictSize) {
                    entry = w + w.charAt(0);
                } else {
                    return null;
                }
            }

            result += entry;

            // Add w+entry[0] to the dictionary.
            dictionary[dictSize++] = w + entry.charAt(0);

            w = entry;
        }
        return result;
    }
};

function DecompressDataClientSide(data) {
    var result = [];
    for (var i = 0; i < data.length; i++) {
        result[i] = LZW.decompress(data[i]);
    }
    return result;
}

function ShowLocationList(element, divIDToBeHidden) {
    RemoveSelection($(element).attr('id'));
    if ($('.ui-loader').css('display') == 'block') {
        return;
    }
    else if (!CheckUserRight('Change Location', enumUserRight["Yes"])) {
        return;
    }

    if (document.getElementById('footerReport') != null) {
        document.getElementById('footerReport').style.display = 'none';
    }

    var pageHeight = $(window).height();
    var headerHeight = $('div[data-role="header"]').height();
    var footerHeight = $('div[data-role="footer"]').height();
    var contentHeight = pageHeight - headerHeight - footerHeight - 5;
    if (document.getElementById('divEmpty') != null) {
        $('#divEmpty').hide();
    }

    $('input[data-type="search"]').val(""); // to clear the input text in Filter.
    $('input[data-type="search"]').trigger("change"); // to trigger the change.
    $("#" + divIDToBeHidden).hide();
    $("#divLocationList").show();
    $('#ulLocationList').height(contentHeight - $('#divLocationList').find('div[data-role="header"]').height() - $('#divLocationList').find('form[class="ui-filterable"]').height());
    if ($(window).width() < 800) {
        $('#divLocationList').width($(window).width());
        document.getElementById('divLocationList').style.marginLeft = "0px";
    }
    else {
        $('#divLocationList').width('300px');
        var leftUl = (($(window).width() - $('#divLocationList').width()) / 2).toString() + "px";

        document.getElementById('divLocationList').style.marginLeft = leftUl;
    }
}

function SelectLocation(locationID, localLocationName) {
    locationName = localLocationName;
    sessionStorage.ReportLocationID = locationID;
    $("#selectLocation").text(locationName.toLowerCase());
    $("#divLocationList").hide();
    callAJAXFunction();
}

function constructLocationList() {
    if (sessionStorage.Session_LocationMaster != null) {
        var locationMaster = eval(sessionStorage.Session_LocationMaster); // data of Location is fetched from session.
        dataSourceLocation = new kendo.data.DataSource({
            schema: {
                model: {
                    id: 'LocationID'
                }
            },
            data: eval(locationMaster)
        });
        dataSourceLocation.read();

        if (typeof (sessionStorage.ReportLocationID) == "undefined") {
            if (dataSourceLocation.get(sessionStorage.defaultLocationID) == undefined) {
                sessionStorage.ReportLocationID = "0";
            }
            else {
                sessionStorage.ReportLocationID = sessionStorage.defaultLocationID;
            }

        }

        locationName = (dataSourceLocation.get(sessionStorage.ReportLocationID)).LocationName;
        var strLocation = "";
        for (var i = 0; i < locationMaster.length; i++) {
            strLocation += '<li style="background-image: none;" onclick="SelectLocation(\'' + locationMaster[i].LocationID + '\', \'' + locationMaster[i].LocationName + '\')" data-theme="a" data-icon="false">' +
			'<a href="">' + locationMaster[i].LocationName.toLowerCase() + '</a></li>';
        }

        $("#ulLocationList").append(strLocation).listview('refresh');
    }
}

function CloseLocationList(divIDToBeShown) {
    $("#divLocationList").hide();
    $("#" + divIDToBeShown).show();
    if (document.getElementById('footerReport') != null) {
        document.getElementById('footerReport').style.display = 'block';
    }
}

function RemoveSelection(elementID) {
    setTimeout(function () {
        $('#' + elementID).removeClass('ui-btn-active');
    }, 200);
}

function CheckUserRight(applicationName, iUserRight) {
    var dataSourceUserRights = CreateDataSource("ApplicationName", sessionStorage.applicationDetails);
    dataSourceUserRights.filter({ field: "ApplicationName", operator: "eq", value: applicationName });
    var UserRights = [];
    if ((dataSourceUserRights.view()).length > 0) {
        UserRights = (dataSourceUserRights.view())[0];
    }
    else {
        return false;
    }

    switch (iUserRight) {
        case enumUserRight["Add"]:
            if (UserRights.ApplicationWeight == 1 || UserRights.ApplicationWeight == 3 || UserRights.ApplicationWeight == 5 || UserRights.ApplicationWeight == 7) {
                return true;
            }
            break;
        case enumUserRight["Modify"]:
            if (UserRights.ApplicationWeight == 2 || UserRights.ApplicationWeight == 3 || UserRights.ApplicationWeight == 6 || UserRights.ApplicationWeight == 7) {
                return true;
            }
            break;
        case enumUserRight["Delete"]:
            if (UserRights.ApplicationWeight == 4 || UserRights.ApplicationWeight == 5 || UserRights.ApplicationWeight == 6 || UserRights.ApplicationWeight == 7) {
                return true;
            }
            break;
        case enumUserRight["Yes"]:
            if (UserRights.ApplicationWeight == 8) {// for secure || UserRights.ApplicationWeight == 48
                return true;
            }
            break;
        case enumUserRight["View"]:
            if (UserRights.ApplicationWeight == 16 || UserRights.ApplicationWeight == 48) {
                return true;
            }
            break;
        case enumUserRight["Print"]:
            if (UserRights.ApplicationWeight == 32 || UserRights.ApplicationWeight == 48) {
                return true;
            }
            break;
        case enumUserRight["Secure"]:
            if (UserRights.ApplicationWeight == 48) {
                return true;
            }
            break;
    }
    return false;
}

function CreateDataSource(ID, Data) {
    var KendoDataSource;
    if (ID != "") {
        KendoDataSource = new kendo.data.DataSource({
            schema: {
                model: {
                    id: ID
                }
            },
            data: eval(Data),
            error: function (e) {
                alert("Error in" + ID);
            },
            change: function () {
            }
        });
    }
    else {
        KendoDataSource = new kendo.data.DataSource({
            data: eval(Data),
            error: function (e) {
                alert("Error in" + ID);
            },
            change: function () {
            }
        });
    }

    return KendoDataSource;
}

function GetYMD(d, strNumberSystem) {
    var strYear = d.getFullYear().toString().slice(2);
    var strMonth = (d.getMonth() + 1).toString();
    var strYMD = "";
    switch (strNumberSystem) {

        case "17": // Yearly
            var strlength = strYear.length;
            strYMD = (strlength == 1 ? "0" : "") + strYear;
            break;

        case "18": // Monthly
            var strlength = strMonth.length;
            strMonth = (strlength == 1 ? "0" : "").toString() + strMonth;
            strYMD = strYear + strMonth;
            break;

        case "19": // Daily
            var strlength = strMonth.length;
            strMonth = (strlength == 1 ? "0" : "") + strMonth;
            strDay = d.getDate().toString();
            strlength = strDay.length;
            strDay = (strlength == 1 ? "0" : "") + strDay;
            strYMD = strYear + strMonth + strDay;
            break;

        default:
            var strlength = strYear.length;
            strYMD = (strYear.Length == 1 ? "0" : "") + strYear;
    }

    return strYMD;
}

function SaveOfflineData() {
    if (typeof (localStorage.OfflineSaveData) == "undefined" || eval(localStorage.OfflineSaveData).length == 0) {
        return;
    }

    var blFoundOfflineData = false;
    var arrOfflineSaveData = eval(localStorage.OfflineSaveData);
    var strIndex = "";
    var customerDetails = "";

    for (var index = 0; index < arrOfflineSaveData.length; index++) {
        if (arrOfflineSaveData[index].SelfHostedIPAddress == localStorage.selfHostedIPAddress) {
            if (!blFoundOfflineData) {
                showLoader($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=SavingOfflineDataStartMessage]").attr('Value'));
                //alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=SavingOfflineDataStartMessage]").attr('Value'));
            }

            blFoundOfflineData = true;

            strIndex += index.toString() + ",";
            var var_UserID = arrOfflineSaveData[index].UserID.toString();
            var var_VoucherID = arrOfflineSaveData[index].VoucherID.toString();
            var numberSystemID = arrOfflineSaveData[index].NumberSystemID.toString();
            var var_LocationID = arrOfflineSaveData[index].LocationID.toString();
            var hashString = arrOfflineSaveData[index].HashString;

            var totalAmountSave = arrOfflineSaveData[index].TotalAmount;

            var strQuery = "SELECT CONVERT(nvarchar(MAX), GETDATE(), 101) + ' ' + CONVERT(varchar(5), GetDate(), 108) As SystemDateValue;";
            strQuery += "select SerialNumber,DataLastChanged,BillAmount from SaleHeader where TableName='" + arrOfflineSaveData[index].TableName + "' and Status<>2 ; ";
            if (arrOfflineSaveData[index].CustomerID != "") {
                strQuery += "select CompanyName, Address1, Address2, Address3, CityID, StateID, CountryID, Pincode, StreetNumber, StreetID, LocalityID from CustomerMaster where " +
										"CustomerID='" + arrOfflineSaveData[index].CustomerID + "';";
            }

            $.support.cors = true;
            $.ajax({
                type: "POST",
                url: "http://" + localStorage.selfHostedIPAddress + "/ExecuteMobiData",
                data: JSON.stringify(
								{ 'dllName': 'RanceLab.Mobi.ver.1.0.dll', 'className': 'Mobi.MobiHelper', 'methodName': 'GetDataFromMultipleQuery', 'parameterList': '{"strQuery":"' + strQuery + '" , "strListDataTypesTemp" : ""}', 'xmlAvailable': false }
						),
                contentType: "application/json; charset=utf-8", // content type sent to server
                dataType: "json", //Expected data format from server
                processdata: true, //True or False
                async: false,
                success: function (data) {
                    var_currentServerDatetime = eval((data.ExecuteMobiDataResult)[0])[0].SystemDateValue;
                    var decompData = eval((data.ExecuteMobiDataResult)[1]);
                    if (decompData.length > 0) {
                        arrOfflineSaveData[index].SerialNumber = decompData[0].SerialNumber; // when SerialNumber exists against CurrentTableName.
                    }

                    if (arrOfflineSaveData[index].CustomerID != "") {
                        customerDetails = eval((data.ExecuteMobiDataResult)[2]);
                    }
                },
                error: function () {
                    alert("Error: JScriptIndex Error.");
                }
            });

            var serialNumberField = "";
            if (arrOfflineSaveData[index].SerialNumber == 0) {
                serialNumberField = "SerialNumber";
            }
            else {
                serialNumberField = arrOfflineSaveData[index].SerialNumber.toString();
            }

            var data = arrOfflineSaveData[index].ArrData;

            if (data.length <= 0) {
                return;
            }

            var sumQuantity = 0;
            var sumtax = 0;
            for (var i = 0; i < data.length; i++) { // looping for each item in "Order List".
                sumQuantity += parseInt(data[i].Quantity); // calculate Total Quantity of Products in "Order List".
                sumtax += parseFloat(data[i].TaxAmount);
            }

            var billAmount = totalAmountSave;
            var roundOffAmount = 0;

            var clsXmlUtilityObject = new clsXmlUtility();
            if (arrOfflineSaveData[index].SerialNumber == 0) {
                clsXmlUtilityObject.AddToList("Table", "saleheader");
                clsXmlUtilityObject.AddToList("IDColumnName", "SerialNumber");
                clsXmlUtilityObject.AddToList("KeyName", "SerialNumber");
                clsXmlUtilityObject.AddToList("GenerateID", "Yes");
                clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);
                clsXmlUtilityObject.GenerateValidXML("voucherdate", var_currentServerDatetime);
                clsXmlUtilityObject.GenerateValidXML("vchidymd", GetYMD(new Date(), numberSystemID));
                clsXmlUtilityObject.AddToList("GenerateVchNumber", var_VoucherID);
                clsXmlUtilityObject.AddToList("VourcherDate", var_currentServerDatetime);
                clsXmlUtilityObject.AddToList("VchYMD", GetYMD(new Date(), numberSystemID));
                clsXmlUtilityObject.AddToList("StartDate", arrOfflineSaveData[index].FinancialYearStart);
                clsXmlUtilityObject.AddToList("EndDate", arrOfflineSaveData[index].FinancialYearEnd);
                clsXmlUtilityObject.GenerateValidXML("vchnumber", null, clsXmlUtilityObject.AttributeListObject);
                clsXmlUtilityObject.GenerateValidXML("voucherid", var_VoucherID);
                clsXmlUtilityObject.GenerateValidXML("userid", var_UserID);
                clsXmlUtilityObject.GenerateValidXML("sessionid", arrOfflineSaveData[index].SessionID);
                clsXmlUtilityObject.GenerateValidXML("layoutid", arrOfflineSaveData[index].LayoutID);
                clsXmlUtilityObject.GenerateValidXML("tablename", arrOfflineSaveData[index].TableName);
                clsXmlUtilityObject.AddToList("FKey", "Y");
                clsXmlUtilityObject.GenerateValidXML("vchidprefix", "SerialNumber", clsXmlUtilityObject.AttributeListObject);
                clsXmlUtilityObject.GenerateValidXML("billamount", billAmount.toString());
                clsXmlUtilityObject.GenerateValidXML("qtytotal", sumQuantity.toString());
                clsXmlUtilityObject.GenerateValidXML("subtotal", totalAmountSave.toString());
                clsXmlUtilityObject.GenerateValidXML("taxtotal", sumtax.toString());
                clsXmlUtilityObject.GenerateValidXML("batchid", arrOfflineSaveData[index].BatchID);
                clsXmlUtilityObject.GenerateValidXML("salemode", "1");
                clsXmlUtilityObject.GenerateValidXML("noofpax", arrOfflineSaveData[index].NoOfPax.toString());
                if (arrOfflineSaveData[index].CustomerID != "") {
                    clsXmlUtilityObject.GenerateValidXML("customerid", arrOfflineSaveData[index].CustomerID);
                    clsXmlUtilityObject.GenerateValidXML("companyname", customerDetails[0].CompanyName);
                    clsXmlUtilityObject.GenerateValidXML("address1", customerDetails[0].Address1);
                    clsXmlUtilityObject.GenerateValidXML("address2", customerDetails[0].Address2);
                    clsXmlUtilityObject.GenerateValidXML("address3", customerDetails[0].Address3);
                    clsXmlUtilityObject.GenerateValidXML("cityid", customerDetails[0].CityID.toString());
                    clsXmlUtilityObject.GenerateValidXML("stateid", customerDetails[0].StateID.toString());
                    clsXmlUtilityObject.GenerateValidXML("countryid", customerDetails[0].CountryID.toString());
                    clsXmlUtilityObject.GenerateValidXML("pincode", customerDetails[0].Pincode.toString());
                    clsXmlUtilityObject.GenerateValidXML("StreetNumber", customerDetails[0].StreetNumber.toString());
                    clsXmlUtilityObject.GenerateValidXML("StreetID", customerDetails[0].StreetID.toString());
                    clsXmlUtilityObject.GenerateValidXML("LocalityID", customerDetails[0].LocalityID.toString());
                }
                clsXmlUtilityObject.GenerateValidXML("createlocationid", var_LocationID);
                clsXmlUtilityObject.GenerateValidXML("locationid", var_LocationID);
                clsXmlUtilityObject.GenerateValidXML("modifylocationid", var_LocationID);
                if (arrOfflineSaveData[index].ServiceModeID != "") {
                    clsXmlUtilityObject.GenerateValidXML("servicemodeid", arrOfflineSaveData[index].ServiceModeID);
                }
                clsXmlUtilityObject.GenerateValidXML("stationid", arrOfflineSaveData[index].StationID);
                clsXmlUtilityObject.GenerateValidXML("status", "0");
            }
            else if (customerDetails != "" || arrOfflineSaveData[index].NoOfPax.toString() != "1" || arrOfflineSaveData[index].ServiceModeID != "") {// update SaleHeader : ISSUE 0017087
                clsXmlUtilityObject.AddToList("Table", "SaleHeader");
                clsXmlUtilityObject.GenerateValidXML("Update", null, clsXmlUtilityObject.AttributeListObject, false);

                if (arrOfflineSaveData[index].NoOfPax.toString() == "0") {
                    clsXmlUtilityObject.GenerateValidXML("noofpax", "1");
                }
                else {
                    clsXmlUtilityObject.GenerateValidXML("noofpax", arrOfflineSaveData[index].NoOfPax.toString());
                }

                if (arrOfflineSaveData[index].ServiceModeID != "") {
                    clsXmlUtilityObject.GenerateValidXML("servicemodeid", arrOfflineSaveData[index].ServiceModeID);
                }
                if (customerDetails != "") {
                    if (arrOfflineSaveData[index].CustomerID != "") {
                        clsXmlUtilityObject.GenerateValidXML("customerid", arrOfflineSaveData[index].CustomerID);
                        clsXmlUtilityObject.GenerateValidXML("LinkCustomerID", arrOfflineSaveData[index].CustomerID);
                    }
                    clsXmlUtilityObject.GenerateValidXML("companyname", customerDetails[0].CompanyName);
                    clsXmlUtilityObject.GenerateValidXML("address1", customerDetails[0].Address1);
                    clsXmlUtilityObject.GenerateValidXML("address2", customerDetails[0].Address2);
                    clsXmlUtilityObject.GenerateValidXML("address3", customerDetails[0].Address3);
                    clsXmlUtilityObject.GenerateValidXML("cityid", customerDetails[0].CityID.toString());
                    clsXmlUtilityObject.GenerateValidXML("stateid", customerDetails[0].StateID.toString());
                    clsXmlUtilityObject.GenerateValidXML("countryid", customerDetails[0].CountryID.toString());
                    clsXmlUtilityObject.GenerateValidXML("pincode", customerDetails[0].Pincode.toString());
                    clsXmlUtilityObject.GenerateValidXML("StreetNumber", customerDetails[0].StreetNumber.toString());
                    clsXmlUtilityObject.GenerateValidXML("StreetID", customerDetails[0].StreetID.toString());
                    clsXmlUtilityObject.GenerateValidXML("LocalityID", customerDetails[0].LocalityID.toString());
                }
                clsXmlUtilityObject.AddToList("SerialNumber", serialNumberField);
                clsXmlUtilityObject.GenerateValidXML("where", null, clsXmlUtilityObject.AttributeListObject);
            }
            //Closing Insert tag of saleheader table....
            clsXmlUtilityObject.EndNode();

            var var_KOTPrinter;
            var var_StationID;
            var i = 0;
            var strTemp;
            var checkCancellationReasonExistance = 0;
            var j = 0;
            do {
                if (data[j].CancellationReasonID.toString() == 0) {
                    checkCancellationReasonExistance = 0;
                }
                else {
                    checkCancellationReasonExistance = 1;
                    break;
                }
                j++;
            } while (j < data.length);

            do {
                var_KOTPrinter = data[i].KOTPrinter;
                var_StationID = data[i].StationID;
                //				if (checkCancellationReasonExistance == 0) {
                do {
                    strTemp = "KOT" + i;
                    clsXmlUtilityObject.AddToList("Table", "restkot");
                    clsXmlUtilityObject.AddToList("IDColumnName", "SerialNumber");
                    clsXmlUtilityObject.AddToList("KeyName", strTemp);
                    clsXmlUtilityObject.AddToList("GenerateID", "Yes");
                    clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);
                    clsXmlUtilityObject.GenerateValidXML("kotdatetime", var_currentServerDatetime);
                    clsXmlUtilityObject.GenerateValidXML("deliverydatetime", var_currentServerDatetime);
                    clsXmlUtilityObject.GenerateValidXML("createlocationid", var_LocationID);
                    clsXmlUtilityObject.GenerateValidXML("locationid", var_LocationID);
                    clsXmlUtilityObject.GenerateValidXML("modifylocationid", var_LocationID);
                    clsXmlUtilityObject.GenerateValidXML("sessionid", arrOfflineSaveData[index].SessionID);
                    //Closing Insert tag of restkot table....
                    clsXmlUtilityObject.EndNode();
                    do {
                        data[i].KOTKey = strTemp;
                        i++;
                    } while (i < data.length && var_KOTPrinter == data[i].KOTPrinter && var_StationID == data[i].StationID);
                } while (i < data.length && var_KOTPrinter == data[i].KOTPrinter && var_StationID == data[i].StationID);
                //				}
                //				else {
                //					i++;
                //				}
            } while (i < data.length);


            i = 0;
            do {
                if (data[i].SrlNo == 0) {
                    clsXmlUtilityObject.AddToList("Table", "saledetail");
                    clsXmlUtilityObject.AddToList("IDColumnName", "SrlNo");
                    clsXmlUtilityObject.AddToList("GenID8Remote", "Yes");
                    clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);

                    if (arrOfflineSaveData[index].SerialNumber == 0) {
                        clsXmlUtilityObject.AddToList("FKey", "Y");
                        clsXmlUtilityObject.GenerateValidXML("serialnumber", "SerialNumber", clsXmlUtilityObject.AttributeListObject);
                    }
                    else {
                        clsXmlUtilityObject.GenerateValidXML("serialnumber", arrOfflineSaveData[index].SerialNumber.toString(), clsXmlUtilityObject.AttributeListObject);
                    }

                    var rootProduct = data[i].OrderID.toString() + data[i].ProductID.toString();
                    if (data[i].RootProduct.toString() == rootProduct) {
                        clsXmlUtilityObject.GenerateValidXML("saletype", "4");
                    }
                    else {
                        clsXmlUtilityObject.GenerateValidXML("saletype", "5");
                    }

                    clsXmlUtilityObject.GenerateValidXML("productid", data[i].ProductID.toString());
                    clsXmlUtilityObject.GenerateValidXML("childid", "0000");
                    clsXmlUtilityObject.GenerateValidXML("locationcode", "000");
                    clsXmlUtilityObject.GenerateValidXML("modifierproductid", data[i].RootProductID.toString());
                    clsXmlUtilityObject.GenerateValidXML("quantity", data[i].Quantity.toString());
                    clsXmlUtilityObject.GenerateValidXML("mrp", data[i].Rate.toString()); //from Menu
                    clsXmlUtilityObject.GenerateValidXML("warehouseid", data[i].WarehouseID.toString()); //from Menu
                    clsXmlUtilityObject.GenerateValidXML("salerate", data[i].Rate.toString()); //from Menu			
                    clsXmlUtilityObject.GenerateValidXML("inputrate", data[i].Rate.toString()); //from Menu
                    clsXmlUtilityObject.GenerateValidXML("salespersonid", data[i].SalesPersonID.toString()); // salespersonid
                    clsXmlUtilityObject.GenerateValidXML("taxid", data[i].TaxID.toString()); //from Menu
                    clsXmlUtilityObject.GenerateValidXML("taxrate", data[i].TaxRate);
                    clsXmlUtilityObject.GenerateValidXML("includeinrate", data[i].IncludeInRate); //from Tax Master 
                    clsXmlUtilityObject.GenerateValidXML("taxid1", data[i].TaxID1.toString()); //from Tax Child
                    clsXmlUtilityObject.GenerateValidXML("taxrate1", data[i].TaxRate1); //from Tax Child
                    clsXmlUtilityObject.GenerateValidXML("taxamount1", data[i].TaxAmount1);
                    clsXmlUtilityObject.GenerateValidXML("taxid2", data[i].TaxID2.toString()); //from Tax Child
                    clsXmlUtilityObject.GenerateValidXML("taxrate2", data[i].TaxRate2); //from Tax Child
                    clsXmlUtilityObject.GenerateValidXML("taxamount2", data[i].TaxAmount2);
                    clsXmlUtilityObject.GenerateValidXML("taxid3", data[i].TaxID3.toString()); //from Tax Child
                    clsXmlUtilityObject.GenerateValidXML("taxrate3", data[i].TaxRate3); //from Tax Child
                    clsXmlUtilityObject.GenerateValidXML("taxamount3", data[i].TaxAmount3);
                    clsXmlUtilityObject.GenerateValidXML("taxid4", data[i].TaxID4.toString()); //from Tax Child
                    clsXmlUtilityObject.GenerateValidXML("taxrate4", data[i].TaxRate4); //from Tax Child
                    clsXmlUtilityObject.GenerateValidXML("taxamount4", data[i].TaxAmount4);
                    clsXmlUtilityObject.GenerateValidXML("finalsalerate", data[i].FinalSaleRate.toString());
                    clsXmlUtilityObject.GenerateValidXML("printerid", data[i].KOTPrinter);
                    clsXmlUtilityObject.GenerateValidXML("stationid", data[i].StationID);
                    clsXmlUtilityObject.GenerateValidXML("menuid", data[i].MenuID);
                    clsXmlUtilityObject.GenerateValidXML("seatid", data[i].SeatID.toString());
                    clsXmlUtilityObject.GenerateValidXML("userid", var_UserID);
                    clsXmlUtilityObject.GenerateValidXML("userstationid", arrOfflineSaveData[index].StationID);
                    if (data[i].KOTNumber == 0) {
                        clsXmlUtilityObject.AddToList("FKey", "Y");
                        clsXmlUtilityObject.GenerateValidXML("kotnumber", data[i].KOTKey.toString(), clsXmlUtilityObject.AttributeListObject);
                    }
                    else {
                        clsXmlUtilityObject.GenerateValidXML("kotnumber", data[i].KOTNumber); //check it.
                    }
                    clsXmlUtilityObject.GenerateValidXML("cancellationreasonid", data[i].CancellationReasonID.toString());
                    clsXmlUtilityObject.GenerateValidXML("isprinted", data[i].IsPrinted.toString());
                    clsXmlUtilityObject.GenerateValidXML("Remarks", ""); // from Menu
                    clsXmlUtilityObject.GenerateValidXML("alternateunitid", data[i].UnitID.toString()); // from Menu
                    clsXmlUtilityObject.GenerateValidXML("conversionfactor", "1");
                    clsXmlUtilityObject.GenerateValidXML("unitid", data[i].UnitID.toString()); // from Menu
                    clsXmlUtilityObject.GenerateValidXML("perunitid", data[i].UnitID.toString()); // from Menu
                    clsXmlUtilityObject.GenerateValidXML("inputrateunitid", data[i].UnitID.toString()); // from Menu
                    clsXmlUtilityObject.GenerateValidXML("inputrateperqty", "1");
                    if (data[i].CancellationReasonID != 0) {
                        clsXmlUtilityObject.GenerateValidXML("voiduserid", var_UserID);
                        clsXmlUtilityObject.GenerateValidXML("voiddatetime", var_currentServerDatetime);
                    }
                    clsXmlUtilityObject.EndNode();
                }
                else {
                    clsXmlUtilityObject.AddToList("Table", "saledetail");
                    clsXmlUtilityObject.GenerateValidXML("Update", null, clsXmlUtilityObject.AttributeListObject, false);

                    clsXmlUtilityObject.GenerateValidXML("quantity", data[i].Quantity.toString());
                    clsXmlUtilityObject.GenerateValidXML("alternateqty", data[i].Quantity.toString());
                    clsXmlUtilityObject.GenerateValidXML("taxamount1", data[i].TaxAmount1);
                    clsXmlUtilityObject.GenerateValidXML("taxamount2", data[i].TaxAmount2);
                    clsXmlUtilityObject.GenerateValidXML("taxamount3", data[i].TaxAmount3);
                    clsXmlUtilityObject.GenerateValidXML("taxamount4", data[i].TaxAmount4);
                    clsXmlUtilityObject.GenerateValidXML("salerate", data[i].Rate.toString());
                    //clsXmlUtilityObject.GenerateValidXML("mrp", mrp.toString());
                    clsXmlUtilityObject.GenerateValidXML("mrp", data[i].Rate.toString());
                    clsXmlUtilityObject.GenerateValidXML("finalsalerate", data[i].FinalSaleRate.toString());
                    clsXmlUtilityObject.GenerateValidXML("kotnumber", data[i].KOTNumber); //check it.
                    clsXmlUtilityObject.GenerateValidXML("cancellationreasonid", data[i].CancellationReasonID.toString()); //check it.
                    clsXmlUtilityObject.GenerateValidXML("isprinted", data[i].IsPrinted.toString());
                    clsXmlUtilityObject.GenerateValidXML("inputrate", data[i].InputRate.toString()); //from Menu
                    clsXmlUtilityObject.GenerateValidXML("salespersonid", data[i].SalesPersonID.toString()); // salespersonid
                    if (data[i].CancellationReasonID != 0) {
                        clsXmlUtilityObject.GenerateValidXML("voiduserid", var_UserID);
                        clsXmlUtilityObject.GenerateValidXML("voiddatetime", var_currentServerDatetime);
                    }

                    clsXmlUtilityObject.AddToList("SrlNo", data[i].SrlNo.toString());
                    clsXmlUtilityObject.GenerateValidXML("where", null, clsXmlUtilityObject.AttributeListObject);

                    //Closing update tag of saledetail table....
                    clsXmlUtilityObject.EndNode();
                }

                i++;
            } while (i < data.length);

            if (arrOfflineSaveData[index].SerialNumber != 0) {
                clsXmlUtilityObject.GenerateValidXML("sql", "UPDATE SaleHeader SET QtyTotal = isNull((SELECT SUM(Quantity) FROM SaleDetail WHERE SerialNumber = " +
		arrOfflineSaveData[index].SerialNumber + "),0), SubTotal = isNull((SELECT SUM(FinalSaleAmount) FROM SaleDetail WHERE SerialNumber = " +
		arrOfflineSaveData[index].SerialNumber + "),0), BillAmount = isNull((SELECT SUM(FinalSaleAmount) FROM SaleDetail WHERE SerialNumber = " +
		arrOfflineSaveData[index].SerialNumber + "),0), TaxTotal = isNull((SELECT SUM(TaxAmount) FROM SaleDetail WHERE SerialNumber = " +
		arrOfflineSaveData[index].SerialNumber + "),0) WHERE SerialNumber = " + arrOfflineSaveData[index].SerialNumber + "");
            }
            //Close SQL tag...
            clsXmlUtilityObject.EndNode();

            $.support.cors = true;
            $.ajax({
                async: "false",
                type: "POST", //GET or POST or PUT or DELETE verb
                url: "http://" + localStorage.selfHostedIPAddress + "/InsertUpdateDeleteForJScript",
                data: '{ "sXElement": "' + encodeURIComponent(clsXmlUtilityObject.ToString()) + '",' +
				'"UserID": ' + var_UserID + ',' +
				'"_ApplicayionType": 1,' +
				'"_AllowEntryInDemo": true,' +
				'"_Userrights_Add": 1,' +
				'"_Userrights_Modify": 1,' +
				'"_Userrights_Delete": 1,' +
				'"_DigitAfterDecimalRateAndAmount": 1,' +
				'"bExportData": false,' +
				'"iMLDataType": 10,' +
				'"iCreateLocationID": ' + arrOfflineSaveData[index].ServerLocationID + ',' +
				'"iModifyLocationID": ' + arrOfflineSaveData[index].ServerLocationID + ',' +
				'"iForOrToLocationID": ' + arrOfflineSaveData[index].CustomerLocationID + ',' +
				'"iFromLocationID": ' + arrOfflineSaveData[index].ServerLocationID + ',' +
				'"strReturnKey":  "' + serialNumberField + '",' +
				'"bFromOutside": true,' +
				'"bSecurutyCheck": false,' +
				'"bValidateXML": false,' +
				'"iTimeOut": 600,' +
				'"iCheckNoOfTime": 0,' +
				'"IncomingVersion": "",' +
				'"IncomingHash": "",' +
				'"RemoteInsert": false,' +
				'"CheckForDuplication": false,' +
				'"HashString": "' + hashString + '",' +
				'"iPriority": 0,' +
				'"selfHostedIPAddress": "' + localStorage.selfHostedIPAddress + '" }',

                contentType: "application/json; charset=utf-8", // content type sent to server
                dataType: "json", //Expected data format from server
                processdata: true, //True or False
                async: false,
                success: function (responseData) {
                    if (responseData.InsertUpdateDeleteForJScriptResult) {
                        $.support.cors = true;
                        $.ajax({
                            async: "false",
                            type: "POST",
                            url: "http://" + localStorage.selfHostedIPAddress + "/ExecuteMobiData",
                            data: JSON.stringify(
								{ 'dllName': 'RanceLab.Mobi.ver.1.0.dll', 'className': 'Mobi.MobiHelper', 'methodName': 'PrintKOT', 'parameterList': '{"voucherOptionData":"' +
								encodeURIComponent(arrOfflineSaveData[index].VoucherOption) + '","posPrintOptionData":"' + encodeURIComponent(arrOfflineSaveData[index].POSPrintOptionData) +
								'","serialNumber": "' + arrOfflineSaveData[index].SerialNumber.toString() + '","StationName":"' + arrOfflineSaveData[index].StationName + '", "CreateLocation" : "' +
								var_LocationID.toString() + '","ModifyLocation" : "' + var_LocationID.toString() + '","ForLocation": "' + var_LocationID.toString() +
								'","MLDefaultLocation": "' + var_LocationID.toString() + '", "DateDisplayFormat": "' + arrOfflineSaveData[index].DateFormat + '","TimeDisplayFormat": "' +
								arrOfflineSaveData[index].TimeFormat + '" }', 'xmlAvailable': false
								}),
                            contentType: "application/json; charset=utf-8",
                            dataType: "json",
                            processdata: true,
                            success: function (data) {

                            },
                            error: function (e) {
                                alert("KOT file creation error.");
                            }
                        });
                    }
                },
                error: function () { alert("error."); }
            });
        }
    }

    strIndex = strIndex.split(",");
    for (var i = 0; i < strIndex.length - 1; i++) {
        arrOfflineSaveData.splice(strIndex[i] - i, 1);
    }

    localStorage.OfflineSaveData = kendo.stringify(arrOfflineSaveData);
    if (blFoundOfflineData) {
        HideLoader();
        alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=SavingOfflineDataEndMessage]").attr('Value'));
    }
}

function InsertDataToDayMaster(startDateTime) {
    var var_UserID = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].UserID.toString();
    var var_Password = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].Password;
    var var_MLGUID = eval(sessionStorage.Session_Server_LocationMaster)[0].MLGUID;
    var var_LocationID = eval(sessionStorage.Session_Server_LocationMaster)[0].LocationID.toString();
    var hashString = var_UserID + var_Password + var_MLGUID;

    var clsXmlUtilityObject = new clsXmlUtility();
    clsXmlUtilityObject.AddToList("Table", "DayMaster");
    clsXmlUtilityObject.AddToList("IDColumnName", "DayID");
    clsXmlUtilityObject.AddToList("KeyName", "DayID");
    clsXmlUtilityObject.AddToList("GenerateID", "Yes");
    clsXmlUtilityObject.AddToList("NoOfChar", "6");
    clsXmlUtilityObject.AddToList("Base", "36");
    clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);
    clsXmlUtilityObject.GenerateValidXML("locationid", var_LocationID);
    clsXmlUtilityObject.GenerateValidXML("StartDateTime", kendo.toString(new Date(startDateTime), "yyyy-MM-dd HH:mm:ss"));
    clsXmlUtilityObject.GenerateValidXML("OpenUserID", var_UserID);
    clsXmlUtilityObject.EndNode();
    clsXmlUtilityObject.EndNode();

    $.support.cors = true;
    $.ajax({
        type: "POST",
        url: "http://" + localStorage.selfHostedIPAddress + "/InsertUpdateDeleteForJScript",
        data: '{ "sXElement": "' + encodeURIComponent(clsXmlUtilityObject.ToString()) + '",' +
				'"UserID": ' + var_UserID + ',' +
				'"_ApplicayionType": 1,' +
				'"_AllowEntryInDemo": true,' +
				'"_Userrights_Add": 1,' +
				'"_Userrights_Modify": 1,' +
				'"_Userrights_Delete": 1,' +
				'"_DigitAfterDecimalRateAndAmount": 1,' +
				'"bExportData": false,' +
				'"iMLDataType": -1,' +
				'"iCreateLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' +
				'"iModifyLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' +
				'"iForOrToLocationID": 1,' +
				'"iFromLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' +
				'"strReturnKey": "DayID",' +
				'"bFromOutside": true,' +
				'"bSecurutyCheck": false,' +
				'"bValidateXML": false,' +
				'"iTimeOut": 600,' +
				'"iCheckNoOfTime": 0,' +
				'"IncomingVersion": "",' +
				'"IncomingHash": "",' +
				'"RemoteInsert": false,' +
				'"CheckForDuplication": false,' +
				'"HashString": "' + hashString + '",' +
				'"iPriority": 0,' +
				'"selfHostedIPAddress": "' + localStorage.selfHostedIPAddress + '" }',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        processdata: true,
        success: function (data) {
            sessionStorage.DayID = data.output1;
            RedirectToMenuPage();
        }
    });
}

function btnlogOut_Click() {
    sessionStorage.clear();
    sessionStorage.logout = "true";
    window.location = "index.html";
}

function showLoader(txt) {
    $.mobile.loading('show', {
        text: txt,
        textVisible: true,
        theme: 'b',
        overlay: 'a',
        html: ""
    });
}

function HideLoader() {
    $.mobile.loading('hide');
}