var dropUploader = function () {

    return {
        autodropzone: function (dropzoneId, fileTypes, returnId, maxSize, DeleteUrl) {
        $(document).ready(function () {
          debugger;
                var dispMsg = 'Drop files here <br /> (Only ' + fileTypes + ')  <br />  Size: ' + maxSize + ' Mb';

                var actionUrl = "/Home/UploadLogo/";
                var imgName = "";

                $("#" + dropzoneId).dropzone(
                    {
                        url: actionUrl, // Set the url
                        paramName: "images",
                        autoProcessQueue: true,
                        addRemoveLinks: true,
                        uploadMultiple: false,
                        maxFiles: 1,
                        parallelUploads: 10,
                        dictResponseError: 'Server not Configured',
                        acceptedFiles: fileTypes,
                        dictDefaultMessage: dispMsg,
                        maxFilesize: maxSize,
                        init: function () {
                            var self = this;
                            // config
                            self.options.addRemoveLinks = true;
                            self.options.dictRemoveFile = "Delete";
                            //New file added
                            self.on("addedfile", function (file) {
                                console.log('new file added ', file);
                            });
                            // Send file starts
                            self.on("sending", function (file, xhr, form) {

                                form.append("filePath", DeleteUrl);
                                console.log('upload started', file);
                                $('.meter').show();
                            });

                            // File upload Progress
                            self.on("totaluploadprogress", function (progress) {
                                console.log("progress ", progress);
                                $('.roller').width(progress + '%');
                            });

                            self.on("queuecomplete", function (progress) {
                                $('.meter').delay(999).slideUp(999);
                            });

                            // On removing file
                            self.on("removedfile", function (file) {

                                deleteFromServer(imgName, DeleteUrl);

                                $("#" + returnId).val('');
                                //console.log(file);
                            });

                            self.on("processing", function (file) {

                                self.options.url = actionUrl;
                            });

                            self.on("complete", function (file) {

                                // self.removeFile(file);
                            });
                        },

                        success: function (file, response) {

                            imgName = response.split(';')[1];
                            var Id = response.split(';')[0];

                            $("#" + returnId).val(imgName);
                            $("#InvoiceImg").val(imgName);
                            file.previewElement.classList.add("dz-success");
                            console.log("Successfully uploaded :" + imgName);
                        },
                        error: function (file, response) {
                          debugger;
                            //Error Messages
                            var errMessage = response;
                            // if error for accepted file
                            if (response.indexOf("You can't upload files of this type") > -1) {
                                errMessage = 'Error! File type not supported. <br>Supported files are ' + fileTypes;
                            }

                            // if error for max size excceeds
                            else if (response.indexOf("File is too big") > -1) {

                                var FileSize = maxSize;
                                var FileType = ' Mb';

                                //If Kb
                                if (FileSize < 1) {
                                    FileSize = FileSize * 1000;

                                    FileType = ' Kb';
                                }

                                else {
                                    SizeTxt = FileSize + ' Mb';
                                }

                                errMessage = 'Error! File is too big. <br>Max file size allowed :' + FileSize + FileType + '.';

                            }
                            // if error for max number of files excceeds
                            else if (response.indexOf("You can not upload any more files") > -1) {

                                errMessage = 'Error! No more files allowed to upload.';

                            }
                            // a default error message
                            else if (response.indexOf("<!DOCTYPE") > -1) {

                                errMessage = 'Internal error';

                            }
                            $('#errmsg').html(errMessage);
                            $('#errmsg').fadeIn('slow').delay(3000).fadeOut('slow');
                            this.removeFile(file);
                        }

                        // file.previewElement.classList.add("dz-error");

                    });
            });
        },

    };

}();



function deleteFromServer(file, DeleteUrl) {
    $.ajax({
        type: 'POST',
        url: "/Home/DeleteFile",
        data: { 'filename': file, 'filepath': DeleteUrl },
        success: function (report) {
            console.log(report);
        },
        error: function (report) {
            console.log(report);
        }
    });
}