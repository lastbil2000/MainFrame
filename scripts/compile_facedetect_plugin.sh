cd $1 
#-lhighgui -lcvaux 
gcc -shared -fPIC -std=c99 -I/usr/local/include/opencv -L/usr/local/lib -lcxcore -lhighgui  -lcv -lml -fPIC `pkg-config --cflags --libs gstreamer-0.10 gstreamer-plugins-base-0.10`   -o $2/facedetect.so facedetect.c

if [ $? -ne 1 ]; 
then
	cp $2/facedetect.so $2/../MainFrame/MainFrame/bin/Debug/
else
	exit 1;
fi

