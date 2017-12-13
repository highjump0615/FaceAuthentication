<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="FaceRecog._default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Face</title>
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
		<div class="container">
			<div class="row" id="changeable">
                <div class="col-md-12 text-center" style="font-size:40px;margin-bottom:50px;color:#0000ff;">
                    <h1>Welcome!</h1>
                </div>
				<div class="col-md-12 text-center">
                    <a class="btn green" href="signup.aspx" style="width: 200px; height:50px;font-size:25px;">SignUp</a>
				</div>
				<div class="col-md-12 text-center">
                    <a class="btn red" href="login.aspx" style="width: 200px; height:50px;font-size:25px;">Login</a>
				</div>
			</div>
		</div>
	</div>
	
    <div class="footer">
      <div class="container">
      </div>
    </div>


    <!-- Bootstrap core JavaScript
    ================================================== -->
    <!-- Placed at the end of the document so the pages load faster -->
    <script src="assets/js/jquery.min.js"></script>
    <script src="assets/js/bootstrap.min.js"></script>
    <script src="assets/js/layout.js"></script>
</body>
</html>
