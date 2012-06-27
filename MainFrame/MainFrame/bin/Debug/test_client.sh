export GST_DEBUG=*:2
gst-launch alsasrc  ! queue ! lamemp3enc ! queue ! tcpclientsink host=localhost port=3000
#gst-launch filesrc location=radiohead.mp3 ! tcpclientsink host=localhost port=3000

#gst-launch filesrc location=radiohead.mp3 ! flump3dec ! alsasink
#gst-launch filesrc location=test.m4a ! mp4mux
#gst-launch filesrc location=test.m4a ! qtdemux ! faad ! audioconvert ! audioresample ! alsasink #autoaudiosink
#gst-launch filesrc location=test.m4a ! decodebin ! audioconvert ! faac ! faad ! audioconvert ! audioresample ! autoaudiosink
#gst-launch filesrc location=test.m4a ! mpegtsmux ! rtpmpapay ! udpsink host="127.0.0.1" port=3000
#gst-launch filesrc location=test.m4a ! rtpL16pay ! rtpL16depay ! qtdemux ! faad ! audioconvert ! audioresample ! alsasink  
#gst-launch filesrc location=test.m4a ! tcpclientsink host="127.0.0.1" port=3000
#gst-launch alsasrc ! tcpclientsink host="127.0.0.1" port=3000
