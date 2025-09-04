


//--------  Validation for Alpha character Value------//

function Character(evt) {

    var charCode = (evt.which) ? evt.which : event.keyCode
    if ((charCode > 64 && charCode < 91) || (charCode > 96 && charCode < 123) || charCode == 127 || charCode == 8 || charCode == 32) {
        return true;
    }
    else {
        return false;
    }
}


//--------  Validation for Alpha Numeric Value------//

function AlphaNumeric(evt) {

    var charCode = (evt.which) ? evt.which : event.keyCode
    if (charCode > 64 && charCode < 91 || charCode > 96 && charCode < 123 || charCode > 47 && charCode < 58 || charCode == 127 || charCode == 8 || charCode == 32) {
        return true;
    }
    else {
        return false;
    }
}



//--------  Validation for Numeric Value------//

function Numeric(evt) {

    var charCode = (evt.which) ? evt.which : event.keyCode
    if (charCode > 47 && charCode < 58 || charCode == 127 || charCode == 8) {
        return true;
    }
    else {
        return false;
    }

  
}



//--------  Validation for Decimal Value------//

function Decimal(event) {


    if (event.which == 8 || event.which == 37 || event.which == 38 || event.which == 39 || event.which == 0) {
        return true;
    }

    if ((event.which != 46 || $(this).val().indexOf('.') != -1) && (event.which < 48 || event.which > 57) && event.which != 8) {

        event.preventDefault();
        return false;

    }
    if (($(this).val().indexOf('.') != -1) && ($(this).val().substring($(this).val().indexOf('.'), $(this).val().indexOf('.').length).length > 2)) {

        event.preventDefault();
    }

}





//Allow only alpha charecter a-z A-Z Space Delete BackSpace

function Alpha(evt) {
    var charCode = (evt.which) ? evt.which : event.keyCode
    if ((charCode > 64 && charCode < 91) || (charCode > 96 && charCode < 123) || charCode == 127 || charCode == 8 || charCode == 32) {
        return true;
    }
    else {
        return false;
    }
}

