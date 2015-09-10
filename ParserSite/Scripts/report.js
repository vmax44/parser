$("select").change(function () {
    //alert(getPartIds().toString());
    recalc();
});

function recalc() {
    var parts = getPartIds();
    parts.forEach(function (part) {
        var summ = Array();
        $("select[PartId='" + part + "']").each(function (ind, elem) {
            if (elem.value != "0") {
                summ.push(getPrice(elem));
            }
        });
        //alert(summ);
        $("div[PartId='" + part + "']").text(mid(summ));
    });
}

//Среднее значение массива
function mid(arr) {
    var summ = 0;
    for (i = 0; i < arr.length; i++) {
        summ += arr[i];
    }
    var middle = (summ / arr.length).toFixed(2);
    return middle;
}

//получаем идентификаторы деталей
function getPartIds() {
    var arr = new Array();
    $("tr[PartId]").each(function () {
        arr.push($(this).attr("PartId"));
    });
    return arr;
}

function getPrice(elem) {
    var selected = elem.options.selectedIndex;
    price = elem.options[selected].text.toString().replace(/,/, ".");
    return parseFloat(price);
}

$(document).ready(function () {
    recalc();
});