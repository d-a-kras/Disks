function x(e) {
    e.preventDefault();
    var files = document.getElementById('uploadFile').files;
    var folder = document.getElementById('folder').nodeValue;
    if (files.length > 0) {
        if (window.FormData !== undefined) {
            var data = new FormData();
            for (var x = 0; x < files.length; x++) {
                data.append("file" + x, files[x]);
            }

            $.ajax({
                type: "POST",
                url: "UploadPicture",
                contentType: false,
                processData: false,
                data: data,
                success: function (result) {
                    $('#picturesform').html('');
                    $('#picturesform').append(result);

                },
                error: function (xhr, status, p3) {
                    alert(xhr.responseText);
                }
            });
        } else {
            alert("Браузер не поддерживает загрузку файлов HTML5!");
        }
    }
}
