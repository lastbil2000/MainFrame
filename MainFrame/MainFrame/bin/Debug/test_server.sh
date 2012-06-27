export GST_DEBUG=*:2
gst-launch  tcpserversrc host=192.168.0.18 port=3000 ! decodebin  ! audioconvert ! audioresample ! alsasink
#gst-launch  tcpserversrc host=192.168.0.18 port=3000 ! filesink location = "delme.m4a" # queue ! decodebin ! audioconvert ! audioresample ! alsasink
#gst-launch  tcpserversrc host=192.168.0.18 port=3000 ! audio/x-raw-int, rate=8000, channels=1, width=16, depth=16, endianness=4321, signed=true ! autoaudiosink
#gst-launch filesrc location=delme.m4a ! wavparse ! audioconvert ! autoaudiosink
#audio/x-raw-int, rate=8000, channels=1, endianness=4321, width=8, depth=8, signed=false ! autoaudiosink
#filesink location = "delme.m4a" #
#gst-launch udpsrc port=3000 caps="application/x-rtp, media=(string)audio, clock-rate=(int)90000, encoding-name=(string)MPA, payload=(int)96" ! rtpmpadepay ! mad ! filesink location="delme"
# faad !  audioconvert ! audioresample ! autoaudiosink
#  vorbisdec ! audioconvert ! audioresample ! autoaudiosink
#gst-launch tcpserversrc host="127.0.0.1" port=3000 ! qtdemux ! faad ! audioconvert ! audioresample ! filesink location="delme.pcm"
#gst-launch tcpserversrc host="127.0.0.1" blocksize=1024 port=3000 num-buffers=-1 ! filesink location="delme.pcm"
