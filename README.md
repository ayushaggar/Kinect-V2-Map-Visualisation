# Kinect-V2-Map-Visualisation

Kinect V2 Map Visualisation - This project is made using various technology. 
Firstly kinectV2 is used for gesture and voice recognition. In this application two types of gesture is recognised one is based on visual gesture builder 
and other one is using bone joints as kinect v2 has the capability to capture small bone in hand so using hand gesture like close , open or lasso. 
After that window app has the capability to speak if speaker is on about what is happening in app otherwise you can see on screen as text too.

Secondly, for the UI interface I have not used the Google earth which is widely used before for Map visualisation. As I like open source so want to use 
cesium which is also for me a great opportunity to learn about WEBgl, new technology of HTML 5 which is widely used for visualisation.
Here we can have a full globe view and also can move globe using gesture, search for the place by speaking and switching to aeroplane and street view by saying
fly and walk respectively. For street view I have used Google street view.

Lastly we can take the snapshot of any state in our app by doing lasso hand gesture.

In the window app I have make the UI such that user not feel alone while using app so user can see himself/herself on the screen and when they do gesture 
they can see bone of the body too. Moreover we do not have to open app again we can close or open kinect as many time as we want in app it will reset the 
application. To reset the cesium just click on home button on it.

I have provided the Instruction on how to use application which can be hide by clicking on hide button.

To run the app things required are mentioned in read me file.
Places we can go - INDIA, AMERICA, SANDIEGO, SANFRANCISCO, MOUNTEVEREST, GRANDCANYON, HANOVER, NEWYORK, DELHI, GERMANY, EUROPE, BANGLORE, MUMBAI, GOA, HAMBURG
HILDESHEIM, BELGIUM, AMSTERDAM, SWITZERLAND, Effiel Tower, Great Pyramid, PARIS, PRAGUE, BERLIN, VENICE, PISA, Taj Mahal

Made By - Ayush Aggarwal- IIT kanpur , www.ayushaggarwal.in
Supervised By - Prof. Dr. Monika Sester

 If you have doubt or want to say thanks feel free to contact me at www.ayushaggarwal.in :) 

Steps required to run the APP

Plugged Kinect to Computer
Open Node.js command prompt from start menu in node.js folder or shortcut key at bottom
Type cd Downloads 
Type cd project
Type cd cesium
Type node server.js
Run the app by going to download and click on KinectV2MapVisualisation.exe file.
To run it in browser use http://localhost:8080/Apps/kinect.html (without Kinect) - helps in debugging
