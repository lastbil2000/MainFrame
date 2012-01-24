cd $1 
#-lhighgui -lcvaux 
gcc -o $2/video.so video.c -shared -fPIC -std=c99 -I/usr/local/include/opencv -L/usr/local/lib -lcxcore -lhighgui  -lcv -lml -fPIC `pkg-config --cflags --libs gstreamer-0.10 gstreamer-plugins-base-0.10`   

if [ $? -eq 0 ]; 
then
	echo "VIDEO COMPILED"
	#cp $2/video.so $2/../MainFrame/MainFrame/bin/Debug/
else
	exit 1;
fi

