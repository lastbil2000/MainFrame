export GST_DEBUG=*:2
#v4l2src use-fixed-fps=false ! video/x-raw-yuv,format=\(fourcc\)UYVY,width=320,height=240 ! ffmpegcolorspace ! filesink location="test.m4a"
# video/x-raw-yuv,width=128,height=96,format='(fourcc)'UYVY
gst-launch  udpsrc  port=5000 ! application/x-rtp, clock-rate=90000,payload=96 ! rtpmp4vdepay queue-delay=0 ! ffdec_mpeg4 ! filesink location="test.mpeg"
#gst-launch v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2,width=320,height=240  ! ffmpegcolorspace ! ffenc_mpeg4 ! video/mpeg ! rtpmp4vpay pt=96 ! udpsink host=localhost port=5000 sync=false
#gst-launch avimux name=mux ! filesink location=test0.avi v4l2src ! video/x-raw-yuv,width=640,height=480,framerate=\(fraction\)30000/1001 ! ffmpegcolorspace ! ffenc_mpeg4 ! queue ! mux. alsasrc device=hw:2,0 ! audio/x-raw-int,channels=2,rate=32000,depth=16 ! audioconvert ! lame ! mux.
#gst-launch alsasrc  ! queue ! lamemp3enc ! queue ! tcpclientsink host=localhost port=3000
#gst-launch filesrc location=radiohead.mp3 ! tcpclientsink host=localhost port=3000

#gst-launch filesrc location=radiohead.mp3 ! flump3dec ! alsasink
#gst-launch filesrc location=test.m4a ! mp4mux
#gst-launch filesrc location=test.m4a ! qtdemux ! faad ! audioconvert ! audioresample ! alsasink #autoaudiosink
#gst-launch filesrc location=test.m4a ! decodebin ! audioconvert ! faac ! faad ! audioconvert ! audioresample ! autoaudiosink
#gst-launch filesrc location=test.m4a ! mpegtsmux ! rtpmpapay ! udpsink host="127.0.0.1" port=3000
#gst-launch filesrc location=test.m4a ! rtpL16pay ! rtpL16depay ! qtdemux ! faad ! audioconvert ! audioresample ! alsasink  
#gst-launch filesrc location=test.m4a ! tcpclientsink host="127.0.0.1" port=3000
#gst-launch alsasrc ! tcpclientsink host="127.0.0.1" port=3000
