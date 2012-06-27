#!/bin/bash

cd $1 
#gcc -shared -fPIC `pkg-config  gstreamer-0.10 gstreamer-plugins-base-0.10 --cflags` -o $2/sphinx_stream.so sphinx_stream.c `pkg-config gstreamer-0.10 gstreamer-plugins-base-0.10 --libs` 
gcc `pkg-config gstreamer-0.10 --cflags`  -o $2/sphinx_server.so sphinx_server.c `pkg-config gstreamer-0.10 --libs` -fPIC #-shared
#-shared -fPIC
if [ $? -ne 1 ]; 
then
	echo "SPHINX COMPILED"
	cp $2/sphinx_server.so $2/../MainFrame/MainFrame/bin/Debug/
else
	return 1;
fi

