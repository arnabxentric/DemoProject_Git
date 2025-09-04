$(function () {
    // GET ID OF last row and increment it by one
    var $lastChar = 1, $newRow;
    $get_lastID = function () {

        var $id = $('#example1 tr:last-child td:first-child').attr("id");

        if ($id == null) {
            $newRow = "<tr id='tr'> \
                    <td id='products_0' class='products_0 ' width='12%'><input type='text' value=''  class='producthide input-non-active' name='producthide' readonly /></td> \
                    <td id='productcode_0 'class='productcode_0 ' width='20%'><input type='text' value=''  class='productcodehide input-non-active' name='productcodehide' readonly /></td> \
                    <td id='barcode_0' class='barcode_0 display-non-div' style='display:none;padding:0px;'  width='10%'><input type='text' value=''  class='barcodehide input-non-active' name='barcodehide' readonly /></td> \
					<td id='description_0' class='description_0 display-non-div' style='display:none;padding:0px;'  width='12%'><input type='text' value=''  class='descriptionhide input-non-active' name='descriptionhide' readonly /></td> \
					<td id='quantity_0' class='quantity_0' width='5%'><input type='text' value=''  class='quantityhide input-non-active' name='quantityhide' style='text-align:right;' readonly /></td> \
					<td id='uom_0' class='uom_0' width='8%'><input type='text' value=''  class='uomhide input-non-active' name='uomhide' readonly /></td> \
                    <td id='of_0' class='of_0' width='5%'><input type='text' value=''  class='ofhide input-non-active' name='ofhide' style='text-align:right;' readonly /></td> \
                    <td id='baseuom_0' class='baseuom_0' width='8%'><input type='text' value=''  class='baseuomhide input-non-active' name='baseuomhide' readonly /></td> \
                    <td id='totalquantity_0' class='totalquantity_0' width='5%'><input type='text' value=''  class='totalquantityhide input-non-active' style='text-align:right;' name='totalquantityhide' readonly /></td> \
                    <td id='availability_0' class='availability_0 display-non-div' style='display:none;padding:0px;' width='5%'><input type='hidden' value=''  class='availabilityhide input-non-active' style='text-align:right;' name='availabilityhide' readonly /></td> \
                    <td id='unitprice_0' class='unitprice_0' width='5%'><input type='text' value=''  class='unitpricehide input-non-active' style='text-align:right;' name='unitpricehide' readonly /></td> \
                    <td id='discount_0' class='discount_0' width='5%'><input type='text' value=''  class='discounthide input-non-active' style='text-align:right;' name='discounthide' readonly /></td> \
                    <td id='disamount_0' class='disamount_0' width='5%'><input type='text' value=''  class='disamounthide input-non-active' style='text-align:right;' name='disamounthide' readonly /></td> \
                    <td id='amount_0' class='amount_0' width='8%'><input type='text' value=''  class='amounthide input-non-active' style='text-align:right;' name='amounthide' readonly /></td> \
                    <td id='producthide_0' class='producthide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value=''  class='product' name='product' /></td> \
                    <td id='uomhide_0' class='uomhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value=''  class='uom' name='uom' /></td> \
                    <td id='baseuomhide_0' class='baseuomhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value=''  class='baseuom' name='baseuom' /></td> \
                  	<td width='2%' align='center' style='background-color:#F9F9F9 !important; box-shadow:none !important'><img src='../../Images/icons-181e6a17.png' src='Delete' class='del_ExpenseRow input-non-active' /></td> \
                    <td id='secuom_0' class='secuom_0 display-non-div'><input type='hidden' value=''  class='secuomhide' name='secuomhide'  /></td> \
                    <td id='secuomhide_0' class='secuomhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value=''  class='secuom' name='secuom' /></td> \
                    <td id='secof_0' class='secof_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value=''  class='secofhide' name='secofhide'  /></td> \
                   </tr>"
            return $newRow;
        }
        else {

            var ax = $id.substr(7);

            $lastChar = parseInt($id.substr(9), 10);

            console.log('GET id: ' + $lastChar + ' | $id :' + $id);
            $lastChar = $lastChar + 1;
            $newRow = "<tr id='tr" + $lastChar + "'> \
                    <td id='products_0" + $lastChar + "' class='products_0' width='12%'><input type='text' value=''  class='producthide input-non-active' name='producthide' readonly /></td> \
                    <td id='productcode_0" + $lastChar + "'class='productcode_0 ' width='20%'><input type='text' value=''  class='productcodehide input-non-active' name='productcodehide' readonly /></td> \
                    <td id='barcode_0" + $lastChar + "'class='barcode_0 display-non-div' style='display:none;padding:0px;'  width='10%'><input type='text' value=''  class='barcodehide input-non-active' name='barcodehide' readonly /></td> \
				    <td id='description_0" + $lastChar + "' class='description_0 display-non-div' style='display:none;padding:0px;'  width='12%'><input type='text' value=''  class='descriptionhide input-non-active' name='descriptionhide' readonly /></td> \
					<td id='quantity_0" + $lastChar + "' class='quantity_0' width='5%'><input type='text' value=''  class='quantityhide input-non-active' name='quantityhide' style='text-align:right;' style='text-align:right;' readonly /></td> \
					<td id='uom_0" + $lastChar + "' class='uom_0' width='8%'><input type='text' value=''  class='uomhide input-non-active' name='uomhide' readonly /></td> \
                    <td id='of_0" + $lastChar + "' class='of_0' width='5%'><input type='text' value=''  class='ofhide input-non-active' name='ofhide' style='text-align:right;' style='text-align:right;' readonly /></td> \
                    <td id='baseuom_0" + $lastChar + "' class='baseuom_0' width='8%'><input type='text' value=''  class='baseuomhide input-non-active' name='baseuomhide' readonly /></td> \
                    <td id='totalquantity_0" + $lastChar + "'  class='totalquantity_0' width='5%'><input type='text' value=''  class='totalquantityhide input-non-active' style='text-align:right;' name='totalquantityhide' readonly /></td> \
                    <td id='availability_0" + $lastChar + "' class='availability_0 display-non-div' style='display:none;padding:0px;' width='5%'><input type='hidden' value=''  class='availabilityhide input-non-active' style='text-align:right;' name='availabilityhide' readonly /></td> \
                    <td id='unitprice_0" + $lastChar + "' class='unitprice_0' width='5%'><input type='text' value=''  class='unitpricehide input-non-active' style='text-align:right;' name='unitpricehide' readonly /></td> \
                    <td id='discount_0" + $lastChar + "' class='discount_0' width='5%'><input type='text' value=''  class='discounthide input-non-active' style='text-align:right;' name='discounthide' readonly /></td> \
                    <td id='disamount_0" + $lastChar + "' class='disamount_0' width='5%'><input type='text' value=''  class='disamounthide input-non-active' style='text-align:right;' name='disamounthide' readonly /></td> \
                    <td id='amount_0" + $lastChar + "' class='amount_0' width='8%'><input type='text' value=''  class='amounthide input-non-active' style='text-align:right;' name='amounthide' readonly /></td> \
                    <td id='producthide_0" + $lastChar + "' class='producthide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value=''  class='product' name='product' /></td> \
                    <td id='uomhide_0" + $lastChar + "' class='uomhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value=''  class='uom' name='uom' /></td> \
                    <td id='baseuomhide_0" + $lastChar + "' class='baseuomhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value=''  class='baseuom' name='baseuom' /></td> \
                  	<td width='2%' align='center' style='background-color:#F9F9F9 !important; box-shadow:none !important'><img src='../../Images/icons-181e6a17.png' src='Delete' class='del_ExpenseRow' /></td> \
                    <td id='secuom_0" + $lastChar + "' class='secuom_0 display-non-div'><input type='hidden' value=''  class='secuomhide' name='secuomhide'  /></td> \
                    <td id='secuomhide_0" + $lastChar + "' class='secuomhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value=''  class='secuom' name='secuom' /></td> \
                    <td id='secof_0" + $lastChar + "' class='secof_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value=''  class='secofhide' name='secofhide'  /></td> \
                   </tr>"
            return $newRow;
        }

    }

    $get_lastIDEdit = function (ItemId, ItemCode, BarCode, ItemName, Description, Quantity, UnitId, UnitName, SecUnitId, SecUnitName, SecUnitFormula, UnitFormula, UnitIdSecondary, UnitSecondaryName, TotalQuantity, Price, Discount, DiscountAmount, TotalAmount, Available) {

        //        var DisAmount = 0;
        //        if (TotalAmount != 0 && Discount != 0) {
        //            DisAmount = TotalAmount * Discount / 100;
        //            DisAmount = DisAmount.toFixed(2);
        //        }
        if (ItemId == 0) {
            ItemId = '';
        }
       
        if (Quantity == 0) {
            Quantity = '';
        }
        if (UnitFormula == 0) {
            UnitFormula = '';
        }
        if (UnitIdSecondary == 0) {
            UnitIdSecondary = '';
        }
        if (TotalQuantity == 0) {
            TotalQuantity = '';
        }
        if (Price == 0) {
            Price = '';
        }
        if (Discount == 0) {
            Discount = '';
        }
        if (TotalAmount == 0) {
            TotalAmount = '';
        }
        if (Available == null) {
            Available = '';
        }
        if (DiscountAmount == 0) {
            DiscountAmount = '';
        }
        BarCode = "";
        var $id = $('#example1 tr:last-child td:first-child').attr("id");

        if ($id == null) {
            $newRow = "<tr id='tr'> \
                    <td id='products_0' class='products_0 ' width='12%'><input type='text' value='" + ItemCode + "'  class='producthide input-non-active' name='producthide' readonly /></td> \
                    <td id='productcode_0' class='productcode_0 ' width='20%'><input type='text' value='" + ItemName + "'  class='productcodehide input-non-active' name='productcodehide' readonly /></td> \
                    <td id='barcode_0' class='barcode_0 display-non-div' style='display:none;padding:0px;'  width='10%'><input type='text' value='" + BarCode + "'  class='barcodehide input-non-active' name='barcodehide' readonly /></td> \
					<td id='description_0' class='description_0 display-non-div' style='display:none;padding:0px;'  width='12%'><input type='text' value='" + Description + "'  class='descriptionhide input-non-active' name='descriptionhide' readonly /></td> \
					<td id='quantity_0' class='quantity_0' width='5%'><input type='text' value='" + Quantity + "'  class='quantityhide input-non-active' name='quantityhide' style='text-align:right;' readonly /></td> \
					<td id='uom_0' class='uom_0' width='8%'><input type='text' value='" + UnitSecondaryName + "'  class='uomhide input-non-active' name='uomhide' readonly /></td> \
                    <td id='of_0' class='of_0' width='5%'><input type='text' value='" + UnitFormula + "'  class='ofhide input-non-active' name='ofhide' style='text-align:right;' readonly /></td> \
                    <td id='baseuom_0' class='baseuom_0' width='8%'><input type='text' value='" + UnitName + "'  class='baseuomhide input-non-active' name='baseuomhide' readonly /></td> \
                    <td id='totalquantity_0' class='totalquantity_0'  width='5%'><input type='text' value='" + TotalQuantity + "'  class='totalquantityhide input-non-active' style='text-align:right;' name='totalquantityhide' readonly /></td> \
                    <td id='availability_0' class='availability_0 display-non-div' style='display:none;padding:0px;' width='5%'><input type='hidden' value='" + Available + "'  class='availabilityhide input-non-active' style='text-align:right;' name='availabilityhide' readonly /></td> \
                    <td id='unitprice_0' class='unitprice_0' width='5%'><input type='text' value='" + Price + "'  class='unitpricehide input-non-active' style='text-align:right;' name='unitpricehide' readonly /></td> \
                    <td id='discount_0' class='discount_0' width='5%'><input type='text' value='" + Discount + "'  class='discounthide input-non-active' style='text-align:right;' name='discounthide' readonly /></td> \
                    <td id='disamount_0' class='disamount_0' width='5%'><input type='text' value='" + DiscountAmount + "'  class='disamounthide input-non-active' style='text-align:right;' name='disamounthide' readonly /></td> \
                    <td id='amount_0' class='amount_0' width='8%'><input type='text' value='" + TotalAmount + "'  class='amounthide input-non-active' style='text-align:right;' name='amounthide' readonly /></td> \
                    <td id='producthide_0' class='producthide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + ItemId + "'  class='product' name='product' /></td> \
                    <td id='uomhide_0' class='uomhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + UnitIdSecondary + "'  class='uom' name='uom' /></td> \
                    <td id='baseuomhide_0' class='baseuomhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + UnitId + "'  class='baseuom' name='baseuom' /></td> \
                  	<td width='2%' align='center' style='background-color:#F9F9F9 !important; box-shadow:none !important'><img src='../../Images/icons-181e6a17.png' src='Delete' class='del_ExpenseRow' /></td> \
                    <td id='secuom_0' class='secuom_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + SecUnitName + "'  class='secuomhide' name='secuomhide'  /></td> \
                    <td id='secuomhide_0' class='secuomhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + SecUnitId + "'  class='secuom' name='secuom' /></td> \
                    <td id='secof_0' class='secof_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + SecUnitFormula + "'  class='secofhide' name='secofhide'  /></td> \
				</tr>"
            return $newRow;
        }
        else {

            var ax = $id.substr(7);

            $lastChar = parseInt($id.substr(9), 10);

            console.log('GET id: ' + $lastChar + ' | $id :' + $id);
            $lastChar = $lastChar + 1;
            $newRow = "<tr id='tr" + $lastChar + "'> \
                    <td id='products_0" + $lastChar + "' class='products_0' width='12%'><input type='text' value='" + ItemCode + "'  class='producthide input-non-active' name='producthide' readonly /></td> \
                    <td id='productcode_0" + $lastChar + "' class='productcode_0 ' width='20%'><input type='text' value='" + ItemName + "'  class='productcodehide input-non-active' name='productcodehide' readonly /></td> \
                    <td id='barcode_0" + $lastChar + "' class='barcode_0 display-non-div' style='display:none;padding:0px;'  width='10%'><input type='text' value='" + BarCode + "'  class='barcodehide input-non-active' name='barcodehide' readonly /></td> \
				    <td id='description_0" + $lastChar + "' class='description_0 display-non-div' style='display:none;padding:0px;'  width='12%'><input type='text' value='" + Description + "'  class='descriptionhide input-non-active' name='descriptionhide' readonly /></td> \
					<td id='quantity_0" + $lastChar + "' class='quantity_0' width='5%'><input type='text' value='" + Quantity + "'  class='quantityhide input-non-active' style='text-align:right;' name='quantityhide' readonly /></td> \
					<td id='uom_0" + $lastChar + "' class='uom_0' width='8%'><input type='text' value='" + UnitSecondaryName + "'  class='uomhide input-non-active' name='uomhide' readonly /></td> \
                    <td id='of_0" + $lastChar + "' class='of_0' width='5%'><input type='text' value='" + UnitFormula + "'  class='ofhide input-non-active' name='ofhide' style='text-align:right;' readonly /></td> \
                    <td id='baseuom_0" + $lastChar + "' class='baseuom_0' width='8%'><input type='text' value='" + UnitName + "'  class='baseuomhide input-non-active' name='baseuomhide' readonly /></td> \
                    <td id='totalquantity_0" + $lastChar + "' class='totalquantity_0'  width='5%'><input type='text' value='" + TotalQuantity + "'  class='totalquantityhide input-non-active' style='text-align:right;' name='totalquantityhide' readonly /></td> \
                    <td id='availability_0" + $lastChar + "' class='availability_0 display-non-div' style='display:none;padding:0px;' width='5%'><input type='hidden' value='" + Available + "'  class='availabilityhide input-non-active' style='text-align:right;' name='availabilityhide' readonly /></td> \
                    <td id='unitprice_0" + $lastChar + "' class='unitprice_0' width='10%'><input type='text' value='" + Price + "'  class='unitpricehide input-non-active' style='text-align:right;' name='unitpricehide' readonly /></td> \
                    <td id='discount_0" + $lastChar + "' class='discount_0' width='5%'><input type='text' value='" + Discount + "'  class='discounthide input-non-active' style='text-align:right;' name='discounthide' readonly /></td> \
                    <td id='disamount_0" + $lastChar + "' class='disamount_0' width='5%'><input type='text' value='" + DiscountAmount + "'  class='disamounthide input-non-active' style='text-align:right;' name='disamounthide' readonly /></td> \
                    <td id='amount_0" + $lastChar + "' class='amount_0' width='8%'><input type='text' value='" + TotalAmount + "'  class='amounthide input-non-active' style='text-align:right;' name='amounthide' readonly /></td> \
                    <td id='producthide_0" + $lastChar + "' class='producthide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + ItemId + "'  class='product' name='product' /></td> \
                    <td id='uomhide_0" + $lastChar + "' class='uomhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + UnitIdSecondary + "'  class='uom' name='uom' /></td> \
                    <td id='baseuomhide_0" + $lastChar + "' class='baseuomhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + UnitId + "'  class='baseuom' name='baseuom' /></td> \
                  	<td width='2%' align='center' style='background-color:#F9F9F9 !important; box-shadow:none !important'><img src='../../Images/icons-181e6a17.png' src='Delete' class='del_ExpenseRow' /></td> \
                    <td id='secuom_0" + $lastChar + "' class='secuom_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + SecUnitName + "'  class='secuomhide' name='secuomhide'  /></td> \
                    <td id='secuomhide_0" + $lastChar + "' class='secuomhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + SecUnitId + "'  class='secuom' name='secuom' /></td> \
                    <td id='secof_0" + $lastChar + "' class='secof_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + SecUnitFormula + "'  class='secofhide' name='secofhide'  /></td> \
				</tr>"
            return $newRow;
        }

    }

    $delete_row = function (e) {
        alert(e.closest('tr'));
        e.closest('tr').remove();
        $lastChar = $lastChar - 2;
    };

    //    $('#example1').on('click', '.del_ExpenseRow', function () {

    //        $(this).closest('tr').remove();
    //        $lastChar = $lastChar - 2;
    //    });
    $('#add_ExpenseRow').on('click', '', function () {
        $add_row();
    });
    $add_row = function () {
        $get_lastID();

        $('#example1 tbody').append($newRow);
    };

    $add_rowEdit = function (ItemId, ItemCode, BarCode, ItemName, Description, Quantity, UnitId, UnitName, SecUnitId, SecUnitName, SecUnitFormula, UnitFormula, UnitIdSecondary, UnitSecondaryName, TotalQuantity, Price, Discount, DiscountAmount, TotalAmount, Available) {

        $get_lastIDEdit(ItemId, ItemCode, BarCode, ItemName, Description, Quantity, UnitId, UnitName, SecUnitId, SecUnitName, SecUnitFormula, UnitFormula, UnitIdSecondary, UnitSecondaryName, TotalQuantity, Price, Discount, DiscountAmount,  TotalAmount, Available);

        $('#example1 tbody').append($newRow);
    };
});

