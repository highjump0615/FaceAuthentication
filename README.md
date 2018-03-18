Basic Face Recognition Login
======

> Face recognition user authentication with shopify, ASP.net 

## Overview

### 1. Main Features
- Face recognition authentication  
Signup & Login in camera view on web page...
- Shopify account redirect  
 
### 2. Techniques  
#### 2.1 FaceRecognition (ASP.NET)
##### 2.1.1 Framework
- .NET Framework 4.5
- Web API v5.0.0

##### 2.1.2 Implementing Features
- [Generating Multipass Token](https://github.com/uoc1691/ShopifyMultipassTokenGenerator)  
After signup or login success, it will generate Shopify Mulitpass Token, and then redirect to shopify store with it.  
- Library for face authentication  
  - FaceRecog\lib\faceengine.dll  
  - FaceRecog\fr.dat (not included) 

#### 2.2 Third Party Plugins & Libs
##### 2.2.1 JS Library  
- [HTML5 Webcam](https://github.com/jhuckaby/webcamjs) v1.0.6
- [MediaStreamRecorder](https://github.com/streamproc/MediaStreamRecorder) v1.3.4

#### 2.3 Shopify
- [Shopify Brooklyn theme](https://themes.shopify.com/themes/brooklyn/styles/classic) for web pages
- Used [Multipass](https://help.shopify.com/api/reference/multipass) for loggin in through Other Platform  

## Need to Improve
- Improve features and UI 
