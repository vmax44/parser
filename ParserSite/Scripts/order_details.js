/**
  * Функция для отправки формы средствами Ajax
**/
function AjaxFormRequest_func(form_id, url, func_ok, func_error) {
    jQuery.ajax({
        url: url, //Адрес подгружаемой страницы

        type: "POST", //Тип запроса
        dataType: "html", //Тип данных
        data: jQuery("#" + form_id).serialize(),
        success: func_ok,
        error: func_error
    });
}
function AjaxFormRequest(result_id, form_id, url) {
    AjaxFormRequest_func(
        form_id,
        url,
        function (response) { //Если все нормально
            document.getElementById(result_id).innerHTML = response;
        },
        function (response) { //Если ошибка
            document.getElementById(result_id).innerHTML = "Ошибка при отправке формы";
        }
    )
}

function AjaxRequest_func(url, func_ok, func_error) {
    jQuery.ajax({
        url: url, //Адрес подгружаемой страницы
        type: "GET", //Тип запроса
        dataType: "html", //Тип данных
        success: func_ok,
        error: func_error
    });
}
function AjaxRequest(result_id, url) {
    AjaxRequest_func(
        url,
        function (response) { //Если все нормально
            document.getElementById(result_id).innerHTML = response;
        },
        function (response) { //Если ошибка
            document.getElementById(result_id).innerHTML = 'Ошибка при отправке формы';
        }
    )
}

//Скрипты для создания и удаления запчасти

part_created_ok = function (res) {
    AjaxRequest('parts_list_container', index_parts_action);
    document.getElementById('parts_create_container').innerHTML = res;
}
part_created_error = function (res) {
    document.getElementById('parts_create_container').innerHTML = 'Ошибка при отправке формы';
}

part_deleting_ok = function (res) {
    AjaxRequest('parts_list_container', index_parts_action);
}
part_deleting_error = function (res) {
    document.getElementById('parts_create_container').innerHTML = 'Ошибка при отправке формы';
}

function Parts_Delete_Click() {
    var f_delete = document.forms["parts_delete_form"];
    AjaxFormRequest_func(f_delete.id, f_delete.action, part_deleting_ok, part_deleting_error);
    return false;
}

//Скрипты для элементов ввода
function Killer(whf) {
    name = whf.value;
    whf.parentNode.innerHTML = name;
}
function script_(whf) {
    if (whf.firstChild==null || whf.firstChild.nodeType == 3) {
        name = whf.innerHTML;
        whf.innerHTML = '<input onblur="Killer(this)" type="input">'
        whf.firstChild.value = name;
        whf.firstChild.focus();
    }
}