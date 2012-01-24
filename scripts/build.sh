#!/bin/bash

BASE_DIR=/home/olga/workspace/robot
#BASE_DIR=$1

if [ ! -e $BASE_DIR ]; then
	echo "$BASE_DIR does not exist!"
	exit -1
fi

# compiling the native espeak/TTS library...
echo "Compiling the espeak library"
./compile_espeak.sh $BASE_DIR/native $BASE_DIR/native


# compiling the native face detection library...
echo "Compiling the video library"
./compile_video.sh $BASE_DIR/native $BASE_DIR/native

if [ $? -eq 1 ]; 
then
	echo "haar video library compilation failed"
	exit -1
fi

#echo "BEHÖVER INTE MER"
#exit 0

# compiling the native gst sphinx library...
echo "Compiling the sphinx library"
sphinx/compile_sphinx_plugn.sh $BASE_DIR/native $BASE_DIR/native

if [ $? -ne 1 ]; 
then
	# building sphinx language
	echo "Building the language library"
	# cp $BASE_DIR/MainFrame/LanguageParser/bin/Debug/LanguageParser.exe $BASE_DIR/scripts/sphinx/LanguageParser.exe
	sphinx/create_language_files.sh $BASE_DIR language /home/olga/workspace/robot/MainFrame/MainFrame/bin/Debug
else
	echo " ERROR BUILDING SPHINX PLUGIN :$?";
fi



