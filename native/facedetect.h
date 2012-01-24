
#include "cv.h"
#include "highgui.h"
#include <stdio.h>
#include <ctype.h>

/// function declarations:

int getMaxWidth();

int getMaxHeight();
int init(
	//const char*(*face_detected_callback)(const char *message),
	const char*(*haar_detected_callback)(int x, int y, int width, int height), 
	const char* cascadeFileName, 
	int cameraNO );
void start();
void stop ();

