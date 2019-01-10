$(function () {
    try {
        Dropzone.autoDiscover = false;
        var fileList = new Array;
        var i = 0;
        $(".dropzone").each(function () {
            var upload = $(this);
            upload.dropzone({
                url: routePrefix + "/Admin/Sellers/SaveUploadedFile?type=" + upload.attr("upload-type") + "&parentId=" + sellerId,
                maxFilesize: 12, // MB
                addRemoveLinks: true,
                dictDefaultMessage:
                    '<span class="bigger-150 bolder"><i class="icon-caret-right red"></i> Drop files</span> to upload \
				    <span class="smaller-80 grey">(or click)</span> <br /> \
				    <i class="upload-icon icon-cloud-upload blue icon-3x"></i> \
                    <div class="dz-default dz-message"></div>',
                dictResponseError: 'Error while uploading file!',

                //change the previewTemplate to use Bootstrap progress bars
                previewTemplate: "<div class=\"dz-preview dz-file-preview\">\n  <div class=\"dz-details\">\n    <div class=\"dz-filename\"><span data-dz-name></span></div>\n    <div class=\"dz-size\" data-dz-size></div>\n    <img data-dz-thumbnail />\n  </div>\n  <div class=\"progress progress-small progress-striped active\"><div class=\"progress-bar progress-bar-success\" data-dz-uploadprogress></div></div>\n  <div class=\"dz-success-mark\"><span></span></div>\n  <div class=\"dz-error-mark\"><span></span></div>\n  <div class=\"dz-error-message\"><span data-dz-errormessage></span></div>\n</div>",
                maxFiles: 15,
                parallelUploads: 5,
                dictMaxFilesExceeded: "You can only upload upto 15 images",
                dictRemoveFile: "Delete",
                dictCancelUploadConfirmation: "Are you sure to cancel upload?",
                accept: function (file, done) {
                    if ((file.type).toLowerCase() != "image/jpg" &&
                        (file.type).toLowerCase() != "image/gif" &&
                        (file.type).toLowerCase() != "image/jpeg" &&
                        (file.type).toLowerCase() != "image/png"
                    ) {
                        done("Invalid file");
                    } else {
                        done();
                    }
                },
                init: function () {
                    var thisDropzone = this;
                    $.get(routePrefix + "/Admin/Sellers/GetSellerGallery?id="+sellerId+"&type=" + upload.attr("upload-type"), function (data) {
                        $.each(data, function (key, value) {
                            var mockFile = { name: value.ImageUrl, size: 100000 };
                            thisDropzone.options.addedfile.call(thisDropzone, mockFile);
                            thisDropzone.options.thumbnail.call(thisDropzone, mockFile, routePrefix + "/Images/" + upload.attr("upload-type") + "/" + sellerId + "/" + value.ImageUrl);
                        });

                    });

                    this.on("success", function (file, serverFileName) {
                        fileList[i] = { "serverFileName": serverFileName, "fileName": file.name, "fileId": i };
                        console.log(fileList);
                        i++;

                    });
                    this.on("removedfile", function (file) {
                        var rmvFile = "";
                        for (f = 0; f < fileList.length; f++) {
                            if (fileList[f].fileName == file.name) {
                                rmvFile = fileList[f].serverFileName.Message;
                            }
                        }
                        if (!rmvFile) {
                            rmvFile = file.name;
                        }
                        $.ajax({
                            url: routePrefix + "/Admin/Sellers/DeleteUploadedFile?fileName=" + rmvFile + "&parentId=@Model.Seller.Id" + "&type=SellerGallery",
                            type: "POST",
                        });
                    });
                },
            });
        });


    } catch (e) {
        alert('Dropzone.js does not support older browsers!');
    }
})