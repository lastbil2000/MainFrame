export GST_DEBUG=*:2
#gst-launch v4l2src ! 'video/x-raw-yuv,format=(fourcc)YUY2,width=640,height=480,framerate=30/1' ! \
#queue ! videorate ! 'video/x-raw-yuv,format=(fourcc)YUY2,framerate=30/1' ! \
#theoraenc quality=10 ! queue	! rtptheorapay name=pay0 pt=96 ! udpsink  port=5005
#v4l2src use-fixed-fps=false ! video/x-raw-yuv,format=\(fourcc\)UYVY,width=320,height=240 ! ffmpegcolorspace ! filesink location="test.m4a"
# video/x-raw-yuv,width=128,height=96,format='(fourcc)'UYVY
#gst-launch v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2,width=320,height=240  ! ffmpegcolorspace ! ffenc_mpeg4 ! video/mpeg ! rtpmp4vpay pt=96 ! udpsink host=192.168.0.18 port=5000 sync=false
#                    'video/x-raw-yuv,width=640,height=480,framerate=30/1'
#gst-launch v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2,width=320,height=240,framerate=30/1 ! queue ! x264enc ! rtph264pay name=pay0 pt=96 ! udpsink host=192.168.0.18 port=5004 sync=false
#gst-launch v4l2src ! videorate ! video/x-raw-yuv,format=\(fourcc\)UYVY,width=320,height=240,framerate=7/1 ! theoraenc quality=10 ! queue ! rtptheorapay name=pay0 pt=96	autoaudiosrc ! vorbisenc ! queue ! rtpvorbispay name=pay1 pt=97 ! udpsink host=192.168.0.18 port=5005 sync=false
 
#gst-launch v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2,width=320,height=240 ! ffmpegcolorspace ! ffenc_mpeg4 ! tcpserversink host=192.168.0.18 port=5002
#gst-launch v4l2src ! video/x-raw-yuv,format=\(fourcc\)UYVY,width=320,height=240  ! ffmpegcolorspace !  ffenc_mpeg4 ! video/mpeg  ! rtpmp4vpay pt=96 ! udpsink host=192.168.0.18 port=5003

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
#gst-launch alsasrc ! tcpserversink  port=5005

############################# TEST: #############################3
#gst-launch -v v4l2src ! 'video/x-raw-yuv,width=640,height=480,format=(fourcc)UYVY' !  ffmpegcolorspace !  x264enc !  rtph264pay name=pay0 pt=96 ! multipartmux ! tcpserversink host=192.168.0.18 port=5005

########################## STRÖMMA: #############################333
#"fungerar" :
#gst-launch -e v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2, framerate=10/1, width=320, height=240 ! ffmpegcolorspace !  x264enc !  rtph264pay name=pay0 pt=96 ! multipartmux ! tcpserversink host=192.168.0.18 port=5005

#"fungerar" :
#gst-launch -e v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2, framerate=10/1, width=320, height=240 ! queue ! videorate ! 'video/x-raw-yuv,format=(fourcc)YUY2,framerate=10/1' ! ffmpegcolorspace ! theoraenc quality=10 ! queue	! rtptheorapay name=pay0 pt=96  ! tcpserversink host=192.168.0.18 port=5005

#"fungerar":
#gst-launch v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2, framerate=10/1, width=320, height=240 ! ffmpegcolorspace ! ffenc_mpeg4 ! video/mpeg ! rtpmp4vpay pt=96 ! multipartmux !  tcpserversink port=5005

#"fungerar":
#gst-launch -e v4l2src ! gdppay ! tcpserversink port=5005 sync=false

#gst-launch -v gstrtpbin name=rtpbin videotestsrc pattern=snow ! ffenc_h263p ! rtph263ppay ! queue ! rtpbin.send_rtp_sink_0 rtpbin.send_rtp_src_0 ! udpsink  port=5000 rtpbin.send_rtcp_src_0 ! udpsink port=5001 sync=false async=false udpsrc port=5005 ! rtpbin.recv_rtcp_sink_0

#gst-launch -e v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2, framerate=10/1, width=320, height=240 ! ffmpegcolorspace ! ffenc_mpeg4 ! rtpmp4gpay !   tcpserversink  port=5005

#FUNGERAR mot vlc, men långsammare (!):
#gst-launch v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2, framerate=10/1, width=320, height=240 ! videorate ! ffmpegcolorspace  ! videoscale method=1 ! video/x-raw-yuv,width=120,height=80,framerate=2/1 ! jpegenc quality=20 ! multipartmux ! tcpserversink  port=5005

#FUNGERAR mot vlc:
#gst-launch v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2, framerate=10/1, width=320, height=240 ! ffmpegcolorspace ! jpegenc quality=30 ! multipartmux ! tcpserversink  port=5005
#FUNGERAR mot vlc med stor fördröjning:
#gst-launch v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2, framerate=10/1, width=320, height=240 ! ffmpegcolorspace ! theoraenc ! oggmux ! tcpserversink  port=5005

#gst-launch v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2, framerate=10/1, width=320, height=240 ! ffmpegcolorspace ! x264enc ! qtmux ! tcpserversink  port=5005

# udpsink host =192.168.0.18  auto-multicast=true port=5005


#gst-launch -e  v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2, framerate=10/1, width=320, height=240 ! ffmpegcolorspace ! x264enc ! qtmux ! tcpserversink host=192.168.0.18 port=5005

#gst-launch -e  v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2, framerate=10/1, width=320, height=240 ! ffmpegcolorspace ! x264enc ! video/x-h264 ! rtph264pay pt=96 ! tcpserversink host=192.168.0.18 port=5005

#"Fungerar":
#gst-launch -e  v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2, framerate=10/1, width=320, height=240 ! ffmpegcolorspace ! x264enc byte-stream=true ! h264parse ! rtph264pay pt=96 ! udpsink host=192.168.0.18 port=5005 sync=false
#gst-launch v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2, framerate=10/1, width=320, height=240 ! x264enc ! h264parse ! rtph264pay ! udpsink host=192.168.0.18 port=5005

#kraschar:
#gst-launch -vvv v4l2src ! video/x-raw-yuv  ! videorate  ! ffmpegcolorspace  ! videoscale method=1 ! video/x-raw-yuv,width=320,height=240,framerate=5/1  ! identity single-segment=true ! x264enc !  mpegtsmux ! rtpmp2tpay ! .send_rtp_sink_0 gstrtpbin ! udpsink port=5005

#gst-launch -e v4l2src do-timestamp=true decimate=5 ! video/x-raw-yuv  ! videorate ! ffmpegcolorspace  ! videoscale method=1 ! video/x-raw-yuv,width=120,height=80,framerate=2/1  ! identity single-segment=true ! ffenc_mpeg4 bitrate=32 !  mpegtsmux ! rtpmp2tpay ! .send_rtp_sink_0 gstrtpbin ! udpsink port=5005


gst-launch v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2, framerate=10/1, width=320, height=240 ! videorate skip-to-first=true ! video/x-raw-yuv,framerate=2/1 ! ffmpegcolorspace  ! identity single-segment=true ! ffenc_mpeg4 bitrate=32 !  mpegtsmux ! rtpmp2tpay ! tcpserversink port=5005
 
#fungerar också:
#gst-launch -e v4l2src ! video/x-raw-yuv  ! videorate ! ffmpegcolorspace  ! videoscale method=1 ! video/x-raw-yuv,width=120,height=80,framerate=2/1  ! identity single-segment=true ! ffenc_mpeg4 bitrate=32 !  mpegtsmux ! rtpmp2tpay ! udpsink port=5005

#fungerar också i vlc. lite bättre än nedanstående
#gst-launch -e v4l2src ! video/x-raw-yuv  ! videorate ! ffmpegcolorspace  ! videoscale method=1 ! video/x-raw-yuv,width=320,height=240,framerate=5/1  ! identity single-segment=true ! ffenc_mpeg4 bitrate=32 !  mpegtsmux ! rtpmp2tpay ! udpsink port=5005

#fungerar också i vlc. lite bättre än nedanstående
#gst-launch -e v4l2src ! video/x-raw-yuv, framerate=10/1, width=320, height=240 ! ffmpegcolorspace ! queue ! x264enc byte-stream=true bitrate=32 ! mpegtsmux ! rtpmp2tpay ! udpsink port=5005

#fungerar inte i vlc
#gst-launch v4l2src device=/dev/video0 ! video/x-raw-yuv, framerate=10/1, width=320, height=240 ! ffmpegcolorspace ! x264enc ! queue ! rtph264pay ! udpsink host=192.168.0.18 port=5005

#kraschar vid uppspelning
#gst-launch -e v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2, framerate=10/1, width=320, height=240 ! ffmpegcolorspace ! queue ! x264enc bitrate=195 ! mpegtsmux !  rtpmp2tpay  pt=96 ! udpsink port=5005 sync=false



#fungerar typ i vlc:
#gst-launch -e v4l2src ! video/x-raw-yuv, framerate=10/1, width=320, height=240 ! videorate ! ffmpegcolorspace ! videoscale method=1 ! video/x-raw-yuv,width=480,height=300,framerate=5/1 ! x264enc byte-stream=true bitrate=195 tune=0x00000004 ! mpegtsmux ! rtpmp2tpay ! udpsink port=5005 host=192.168.0.18
#nej:
#gst-launch -e  v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2, framerate=10/1, width=320, height=240 ! queue ! videorate ! 'video/x-raw-yuv,format=(fourcc)YUY2,framerate=10/1'  ! ffmpegcolorspace ! x264enc ! video/x-h264 ! rtph264pay pt=96 ! multipartmux  ! tcpserversink host=192.168.0.18 port=5005
# ! qtmux streamable=true

#  video/x-h264 ! rtph264pay pt=96 ! multipartmux 
###################### SPARA TILL DISK: ###########################################
#FUNGERAR!!:
#gst-launch -e  v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2, framerate=10/1, width=320, height=240 ! queue ! videorate ! 'video/x-raw-yuv,format=(fourcc)YUY2,framerate=10/1'  ! ffmpegcolorspace ! x264enc  ! qtmux ! filesink location=test.mp4

#Fungerar i vlc
#gst-launch -e  v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2, framerate=10/1, width=320, height=240 ! queue ! videorate ! 'video/x-raw-yuv,format=(fourcc)YUY2,framerate=10/1'  ! ffmpegcolorspace ! ffenc_h263p  ! qtmux ! filesink location=test.m4a

#Fungerar i vlc
#gst-launch -e videotestsrc ! video/x-raw-yuv, framerate=25/1, width=640, height=360 ! ffmpegcolorspace ! ffenc_h263p  ! qtmux ! filesink location=test.m4a

#minns inte:
#gst-launch -e avimux name="mux" ! filesink location=test0.avi v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2, framerate=10/1, width=320, height=240 ! ffmpegcolorspace ! ffenc_mpeg4 ! queue ! mux. alsasrc ! audio/x-raw-int,channels=2,rate=32000,depth=16 ! audioconvert ! lame ! mux.

#fungerar ingenstans:
#gst-launch -e v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2, framerate=10/1, width=320, height=240 ! queue ! videorate ! 'video/x-raw-yuv,format=(fourcc)YUY2,framerate=10/1' ! ffmpegcolorspace ! theoraenc quality=10 ! filesink location="test.mpeg"

#fungerar, men inte på teli:
#gst-launch -e videotestsrc ! video/x-raw-yuv, framerate=25/1, width=640, height=360 ! ffmpegcolorspace ! ffenc_mpeg4 ! qtmux ! filesink location=test.m4a

#fungerar, men inte på teli:
#gst-launch -e v4l2src ! video/x-raw-yuv,format=\(fourcc\)YUY2, framerate=10/1, width=320, height=240 ! ffmpegcolorspace ! ffenc_mpeg4 ! qtmux ! filesink location=test.m4a

