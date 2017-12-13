<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="signup.aspx.cs" Inherits="FaceRecog.signup" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <!-- The above 3 meta tags *must* come first in the head; any other head content must come *after* these tags -->
    <meta name="description" content="" />
    <meta name="author" content="" />
    <link rel="icon" href="favicon.ico" />

    <title>SignUp</title>

    <!-- Bootstrap core CSS -->
    <link href="assets/css/bootstrap.min.css" rel="stylesheet" />

    <!-- Custom styles for this template -->
    <link href="assets/css/layout.css" rel="stylesheet" />
    <link href="assets/css/style.css" rel="stylesheet" />

    <!-- HTML5 shim and Respond.js for IE8 support of HTML5 elements and media queries -->
    <script src="assets/js/modernizr.custom.min.js"></script>
	<script src="assets/js/html5shiv.min.js"></script>
	<script src="assets/js/respond.min.js"></script>
</head>
<body>
    <div class="wrap">
		<div class="container" style="background:#555;padding:0;">
			<div class="row" id="changeable">
    			<video id="webcamShow" style="width:100%; height:80%;" autoplay></video>
            </div>
			<div class="form-group">
				<label class="col-sm-5 control-label" style="font-size:18px;margin-bottom:10px;color:#ffffff;">Name : </label>
				<div class="col-sm-7" style="margin-bottom:5px;">
					<div class="input-inline input-medium">
						<div class="input-group">
							<span class="input-group-addon">
								<span class="glyphicon glyphicon-envelope"></span>
							</span>
							<input type="text" id="username" class="form-control" placeholder="username" />
						</div>
					</div>
				</div>
			</div>
			<div class="form-group">
				<label class="col-sm-5 control-label" style="font-size:18px;margin-bottom:10px;color:#ffffff;">Address : </label>
				<div class="col-sm-7" style="margin-bottom:10px;">
					<div class="input-inline input-medium">
						<div class="input-group">
							<span class="input-group-addon">
								<span class="glyphicon glyphicon-envelope"></span>
							</span>
							<input type="text" id="useraddress" class="form-control" placeholder="useraddress" />
						</div>
					</div>
				</div>
			</div>
            <div style="text-align:center;">
                <h4 id="notify_permission"></h4>
            </div>
            <div style="text-align:center;">
                <input id="btn-signup" class="btn green" style="width: 150px; height:40px;font-size:20px;" value="SignUp" />
            </div>
            <div id="my_camera" style="visibility: hidden; position:absolute; bottom:0; right:0"></div>
		</div>
	</div>

    <!-- Bootstrap core JavaScript
    ================================================== -->
    <!-- Placed at the end of the document so the pages load faster -->
    <script src="assets/js/jquery.min.js"></script>
    <script src="assets/js/bootstrap.min.js"></script>
    <script type="text/javascript" src="assets/js/facedetect/jsfeat-min.js"></script>
    <script type="text/javascript" src="assets/js/facedetect/frontalface.js"></script>
    <script type="text/javascript" src="assets/js/facedetect/compatibility.js"></script>
    <script type="text/javascript" src="assets/js/facedetect/profiler.js"></script>
    <script type="text/javascript" src="assets/js/facedetect/dat.gui.min.js"></script>
    <script type="text/javascript" src="assets/js/MediaStreamRecorder.js"> </script>
    <script type="text/javascript" src="assets/js/webcam.js"></script>
</body>
</html>
<script>
    document.getElementById("btn-signup").disabled = true;
    document.getElementById("notify_permission").style = "color:white";
    document.getElementById("notify_permission").innerHTML = "Please share your camera device!";

    var handleHeight = function () {
        var d = 0;
        d = $(window).height() - $('.footer').outerHeight();
        if (d > 0) {
            $('.wrap > .container').height(d);
        }
    };

    ////// Face detection related variables
    var stat = new profiler();
    var options, img_u8, ii_sum, ii_sqsum, ii_tilted, edg, ii_canny;
    var facedetection_canvas, facedetection_ctx;

    ////// This controls the size of the frame data for face detection
    var facedetection_work_size = 160;
    var classifier = jsfeat.haar.frontalface;

    ////// Face Detection Options
    var faceDetectionOption = function () {
        this.min_scale = 2;
        this.scale_factor = 1.15;
        this.use_canny = false;
        this.edges_density = 0.13;
        this.equalize_histogram = true;
    }

    ////// Here we initialize variables
    function initializeFaceDetection(videoWidth, videoHeight) {
        // Face detection initialization
        var scale = Math.min(facedetection_work_size / videoWidth, facedetection_work_size / videoHeight);
        var w = (videoWidth * scale) | 0;
        var h = (videoHeight * scale) | 0;
        img_u8 = new jsfeat.matrix_t(w, h, jsfeat.U8_t | jsfeat.C1_t);
        edg = new jsfeat.matrix_t(w, h, jsfeat.U8_t | jsfeat.C1_t);

        facedetection_canvas = document.createElement('canvas');
        facedetection_canvas.width = w;
        facedetection_canvas.height = h;
        facedetection_ctx = facedetection_canvas.getContext('2d');
        ii_sum = new Int32Array((w + 1) * (h + 1));
        ii_sqsum = new Int32Array((w + 1) * (h + 1));
        ii_tilted = new Int32Array((w + 1) * (h + 1));
        ii_canny = new Int32Array((w + 1) * (h + 1));

        stat.add("haar detector");
        options = new faceDetectionOption();
    }

    $(window).load(function () {
        handleHeight();

        //
        var video = document.getElementById('webcamShow')
          , canvas = document.createElement('canvas')
          , localstream;

        var userMainPhoto
            , userSubPhotos = new Array(4);

        try {
            var attempts = 0;
            var readyListener = function (event) {
                findVideoSize();
            };
            var findVideoSize = function () {
                if (video.videoWidth > 0 && video.videoHeight > 0) {
                    video.removeEventListener('loadeddata', readyListener);
                    onDimensionsReady(video.videoWidth, video.videoHeight);
                } else {
                    if (attempts < 10) {
                        attempts++;
                        setTimeout(findVideoSize, 200);
                    } else {
                        onDimensionsReady(320, 240);
                    }
                }
            };
            var onDimensionsReady = function (width, height) {
                document.getElementById("btn-signup").disabled = false;
                document.getElementById("notify_permission").innerHTML = "";
                initializeFaceDetection(width, height);
//                compatibility.requestAnimationFrame(tick);
            };

            video.addEventListener('loadeddata', readyListener);

            // Older browsers might not implement mediaDevices at all, so we set an empty object first
            if (navigator.mediaDevices === undefined) {
                navigator.mediaDevices = {};
            }

            // Some browsers partially implement mediaDevices. We can't just assign an object
            // with getUserMedia as it would overwrite existing properties.
            // Here, we will just add the getUserMedia property if it's missing.
            if (navigator.mediaDevices.getUserMedia === undefined) {
                navigator.mediaDevices.getUserMedia = function (constraints) {

                    // First get ahold of the legacy getUserMedia, if present
                    var getUserMedia = navigator.webkitGetUserMedia || navigator.mozGetUserMedia;

                    // Some browsers just don't implement it - return a rejected promise with an error
                    // to keep a consistent interface
                    if (!getUserMedia) {
                        document.getElementById("notify_permission").style = "color:red";
                        document.getElementById("notify_permission").innerHTML = "getUserMedia is not implemented in this browser";
                        return Promise.reject(new Error('getUserMedia is not implemented in this browser'));
                    }

                    // Otherwise, wrap the call to the old navigator.getUserMedia with a Promise
                    return new Promise(function (resolve, reject) {
                        getUserMedia.call(navigator, constraints, resolve, reject);
                    });
                }
            }

            navigator.mediaDevices.getUserMedia({ video: true })
            .then(function (stream) {
                var video = document.querySelector('video');
                // Older browsers may not have srcObject
                if ("srcObject" in video) {
                    video.srcObject = stream;
         
                } else {
                    // Avoid using this in new browsers, as it is going away.
                    video.src = window.URL.createObjectURL(stream);
                }

                localstream = stream;
                video.onloadedmetadata = function (e) {
                    video.play();
                };
            })
            .catch(function (err) {
                document.getElementById("notify_permission").style = "color:red";
                document.getElementById("notify_permission").innerHTML = "Could not access the camera!\nError Code: " + error.name;
            });

/*
            compatibility.getUserMedia({ video: true }, function (stream) {
                try {
                    video.src = compatibility.URL.createObjectURL(stream);
                    localstream = stream;
                } catch (error) {
                    video.src = stream;
                }
                setTimeout(function () {
                    video.play();
                }, 500);
            }, function (error) {
                document.getElementById("notify_permission").style = "color:red";
                document.getElementById("notify_permission").innerHTML = "Could not access the camera!\nError Code: " + error.name;
            });
*/


        } catch (error) {
            document.getElementById("notify_permission").style = "color:red";
            document.getElementById("notify_permission").innerHTML = "This browser does not support camera access in HTML5!";
        }

        function sendUserData(blob) {
            var httpRequest = new XMLHttpRequest();
            httpRequest.onreadystatechange = function () {
                if (httpRequest.readyState === 4) {
                    var response = JSON.parse(httpRequest.responseText);
                    if (!response.success) {
                        document.getElementById("btn-signup").disabled = false;
                        document.getElementById("notify_permission").innerHTML = "";
                        alert('Signup failed!\n' + response.message);
                        window.location.reload();
                    }
                    else {
                        document.getElementById("btn-signup").disabled = false;
                        document.getElementById("notify_permission").innerHTML = "";
                        alert('Congratulations for signing up to our community!\n');
                        window.location.replace("default.aspx");
                    }
                }
            };
            httpRequest.open("POST", "api/signup");
            var fd = new FormData();
            fd.append("UserName", $("#username").val());
            fd.append("UserAddress", $("#useraddress").val());
            fd.append("MainPhoto", userMainPhoto.src);
            fd.append("SubsidiaryPhoto1", userSubPhotos[0]);
            fd.append("SubsidiaryPhoto2", userSubPhotos[1]);
            fd.append("SubsidiaryPhoto3", userSubPhotos[2]);
            fd.append("SubsidiaryPhoto4", userSubPhotos[3]);
            fd.append("UserVideo", blob);
            httpRequest.send(fd);
        }

        function dataURItoBlob(dataURI) {
            // convert base64/URLEncoded data component to raw binary data held in a string
            var byteString;
            if (dataURI.split(',')[0].indexOf('base64') >= 0)
                byteString = atob(dataURI.split(',')[1]);
            else
                byteString = unescape(dataURI.split(',')[1]);

            // separate out the mime component
            var mimeString = dataURI.split(',')[0].split(':')[1].split(';')[0];

            // write the bytes of the string to a typed array
            var ia = new Uint8Array(byteString.length);
            for (var i = 0; i < byteString.length; i++) {
                ia[i] = byteString.charCodeAt(i);
            }

            return new Blob([ia], { type: mimeString });
        }

        function takePhoto(index) {
            Webcam.snap(function (data_uri) {
                var blob = dataURItoBlob(data_uri);
                userSubPhotos[index] = blob;
            });            
        }

        function takePhotoArray() {
            <% for (int i = 0; i < 4; i++) {%>
                setTimeout(function () { takePhoto(<%=i%>); }, (Object)(<%=500 * (i + 1)%>));
            <%}%>
        }

        function takeSignupInfo() {
            if ($("#username").val() == '' || $("#useraddress").val() == '')
            {
                document.getElementById("notify_permission").style = "color:blue";
                document.getElementById("notify_permission").innerHTML = "Warning!\nPlease make sure that you've filled out required forms for enrollment.";
                return;
            }

            userMainPhoto = document.querySelector('img') || document.createElement('img');

            var context;
            var width = video.offsetWidth
              , height = video.offsetHeight;

            canvas.width = width;
            canvas.height = height;

            context = canvas.getContext('2d');
            context.drawImage(video, 0, 0, width, height);

            ////// Here we're detecting a face
            facedetection_ctx.drawImage(video, 0, 0, facedetection_canvas.width, facedetection_canvas.height);
            var imageData = facedetection_ctx.getImageData(0, 0, facedetection_canvas.width, facedetection_canvas.height);

            stat.start("haar detector");
            jsfeat.imgproc.grayscale(imageData.data, facedetection_canvas.width, facedetection_canvas.height, img_u8);
            if (options.equalize_histogram) {
                jsfeat.imgproc.equalize_histogram(img_u8, img_u8);
            }
            jsfeat.imgproc.compute_integral_image(img_u8, ii_sum, ii_sqsum, classifier.tilted ? ii_tilted : null);

            if (options.use_canny) {
                jsfeat.imgproc.canny(img_u8, edg, 10, 50);
                jsfeat.imgproc.compute_integral_image(edg, ii_canny, null, null);
            }

            jsfeat.haar.edges_density = options.edges_density;
            var faceRects = jsfeat.haar.detect_multi_scale(ii_sum, ii_sqsum, ii_tilted, options.use_canny ? ii_canny : null, img_u8.cols, img_u8.rows, classifier, options.scale_factor, options.min_scale);
            // This is for storing detected face regions
            faceRects = jsfeat.haar.group_rectangles(faceRects, 1);
            stat.stop("haar detector");

            if (!faceRects[0])
            {
                document.getElementById("notify_permission").style = "color:blue";
                document.getElementById("notify_permission").innerHTML = "Could not detect a face!\nPlease make sure that you're now facing the camera!";
                return;
            }

            document.getElementById("btn-signup").disabled = true;
            userMainPhoto.src = canvas.toDataURL('image/png');

            document.getElementById("notify_permission").style = "color:white";
            document.getElementById("notify_permission").innerHTML = "Please confirm if you're okay with creating instances of your camera device to take a video!";
 
            //turn on photo capture
            Webcam.set({
                width: 640,
                height: 480,
                image_format: 'jpeg',
                jpeg_quality: 100
            });
            Webcam.on('load', function () {
                //turn on video recorder
                mediaRecorder = new MediaStreamRecorder(localstream);
                mediaRecorder.mimeType = 'video/webm';
                mediaRecorder.ondataavailable = function (blob) {
                    //turn off photo capture
                    Webcam.off('load', function () { });
                    Webcam.reset();

                    //send captured data to server
                    sendUserData(blob);
                    document.getElementById("notify_permission").style = "color:white";
                    document.getElementById("notify_permission").innerHTML = "Checking user's photo...";
                };
                document.getElementById("notify_permission").style = "color:white";
                document.getElementById("notify_permission").innerHTML = "Recording a video now ...";
                setTimeout(function () { mediaRecorder.start(2000); }, (Object)(1000));
                setTimeout(function () { mediaRecorder.stop(); }, (Object)(3000));

                //
                takePhotoArray();
            });
            Webcam.attach('#my_camera');
        }
        $("#btn-signup").click(function () {
            takeSignupInfo();
        });
    });
</script>