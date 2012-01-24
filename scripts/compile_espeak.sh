cd $1 

gcc `pkg-config gstreamer-0.10 --cflags` -o $2/espeak_stream.so espeak_stream.c -shared -fPIC `pkg-config gstreamer-0.10 --libs`   

if [ $? -eq 0 ]; 
then
	echo "ESPEAK COMPILED"
	#cp $2/espeak.so $2/../MainFrame/MainFrame/bin/Debug/
else
	exit 1;
fi
