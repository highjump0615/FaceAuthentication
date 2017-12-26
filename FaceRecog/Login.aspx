<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="FaceRecog.Login" %>

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

    <title>Login</title>

    <!-- Bootstrap core CSS -->
    <link href="assets/css/bootstrap.min.css" rel="stylesheet" />

    <!-- Custom styles for this template -->
    <link href="assets/css/layout.css?171219" rel="stylesheet" />
    <link href="assets/css/style.css" rel="stylesheet" />

    <!-- HTML5 shim and Respond.js for IE8 support of HTML5 elements and media queries -->
    <script src="assets/js/modernizr.custom.min.js"></script>
	<script src="assets/js/html5shiv.min.js"></script>
	<script src="assets/js/respond.min.js"></script>
</head>
<body>
    <div class="wrap">
		<div class="container" style="background:#555;padding:0;">
			<div class="row mb-15" id="changeable">
    			<video id="webcam" style="width:100%; height:80%;" autoplay="autoplay"></video>
            </div>
			<div class="form-group">
				<label class="col-sm-5 control-label" style="font-size:18px;margin-bottom:10px;color:#ffffff;">First Name : </label>
				<div class="col-sm-7" style="margin-bottom:5px;">
					<div class="input-inline input-medium">
						<div class="input-group">
							<span class="input-group-addon">
								<span class="glyphicon glyphicon-user"></span>
							</span>
							<input type="text" id="firstname" class="form-control" placeholder="First Name" />
						</div>
					</div>
				</div>
			</div>
			<div style="text-align:center;">
                <h4 id="notify_permission"></h4>
            </div>
            <div style="text-align:center;">
                <button id="btn-login" class="btn red mt-15" style="width: 150px; height:40px;font-size:20px;">Login</button>
            </div>

            <div class="text-center mt-15 notice-link"
              >Not having an account?<a href="signup.aspx<%= Request.Url.Query %>">&nbsp;&nbsp;Sign up</a> here
            </div>
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
</body>
</html>
<script>

    // get parameters from url
    var gstrReturn = '<%= Request.QueryString["returnUrl"] %>';
    var gstrFrom = '<%= Request.QueryString["from"] %>';

    document.getElementById("btn-login").disabled = true;
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
        var video = document.getElementById('webcam')
          , canvas = null;

        var userPhoto;

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
                document.getElementById("btn-login").disabled = false;
                document.getElementById("notify_permission").innerHTML = "";
                initializeFaceDetection(width, height);
//                compatibility.requestAnimationFrame(tick);
            };

            video.addEventListener('loadeddata', readyListener);

            compatibility.getUserMedia({ video: true }, function (stream) {
                try {
                    video.src = compatibility.URL.createObjectURL(stream);
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
        } catch (error) {
            document.getElementById("notify_permission").style = "color:red";
            document.getElementById("notify_permission").innerHTML = "This browser does not support HTML5!";
        }

        function handleResponse(data) {
            // Fail
            if (!data.success) {
                document.getElementById("btn-login").disabled = false;
                document.getElementById("notify_permission").innerHTML = "";
                alert('Login failed!\n' + data.message);

                // Resumes camera
                video.play();
            }
            // Success
            else {
                document.getElementById("btn-login").disabled = false;
                document.getElementById("notify_permission").innerHTML = "";
                alert('Welcome to visiting our site again!\n Hope you will enjoy your journey.');                
                window.location.replace(data.redirectUrl);
            }            
        }

        function handleError(jqXHR, status, error) {
            alert('Error!');
        }

        function takeSnapshot() {
            
            if ($("#firstname").val() == '') {
                document.getElementById("notify_permission").style = "color:blue";
                document.getElementById("notify_permission").innerHTML = "Warning!\nPlease enter your name.";
                return;
            }

            userPhoto = document.querySelector('img') || document.createElement('img');
            var context;
            var width = video.offsetWidth
              , height = video.offsetHeight;

            canvas = canvas || document.createElement('canvas');
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

            if (!faceRects[0]) {
                document.getElementById("notify_permission").style = "color:blue";
                document.getElementById("notify_permission").innerHTML = "Could not detect a face!\nPlease make sure that you're now facing the camera!";
                return;
            }

            document.getElementById("btn-login").disabled = true;
            document.getElementById("notify_permission").style = "color:white";
            document.getElementById("notify_permission").innerHTML = "We're verifying you now. Please wait for a while...";
            userPhoto.src = canvas.toDataURL('image/png');
            video.pause();
            
            $.ajax({
                url: 'api/login',
                type: 'POST',
                data: {
                    UserPhoto: userPhoto.src,
                    UserName: $("#firstname").val(),
                    ReturnUrl: gstrReturn,
                    From: gstrFrom
                },
                dataType: 'json',
                async: false,
                success: handleResponse,
                error: handleError
            });
        }
        $("#btn-login").click(function () {
            takeSnapshot();
        });
    });
</script>
