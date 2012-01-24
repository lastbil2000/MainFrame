#include "cv.h"
#include "highgui.h"
#include <stdio.h>
#include <ctype.h>

// capture object

typedef struct CaptureObject {

	int position;							//position in the array.. usefull?
	CvHaarClassifierCascade* cascade;		//the haar cascade
	const char* cascadeFileName;			//the file containing the haar cascades
	CvMemStorage* storage;					//used for haar operations
	CvRect latestCapture;					//the latest capture for this object
	CvRect bounds;							//bounds where to search in the video frame, or (0,0,0,0) to search the entire frame
	int didCapture;							//did the latest try to capture result in a capture?
	int initialized;						//is this CaptureObject loaded?
	int hasCapturedImage;					//if there is any captured image
	IplImage *capturedImage;				//capturedImage (if any) stored in RGBA format
	int capturedImageWidth;					//the size of the captured image
	int capturedImageHeight;				// width...
	int shouldCapture;						//if set, this object should capture

} CaptureObject;

/// function declarations:

//returns 1 if a process of trying to capture an object is initiated
int _ext_isCapturing(int position);

//returns 1 if a capture was successfull
int _ext_didCapture(int position);

//gets the screen width or 0 if no frame found
int _ext_getScreenWidth();
//gets the sreen height  0 if no frame captured
int _ext_getScreenHeight();
//init the video capture for camera
int _ext_init(int cameraNO );
//starts the video capture
int _ext_start();
//stops the video capture
void _ext_stop ();

//creates and allocates a capture object at position for cascade file
int _ext_createCaptureObject(int position, const char* cascadeFileName);
//set the capture bounds for an object
void _ext_setCaptureObjectBounds(int position, CvRect bounds);
//releases an object at position from memory
void _ext_releaseCaptureObjectBounds(int position);
//sets the object at position to "shouldCapture = 1"
void _ext_capture(int position);
//returns the raw imageData from the last frame or 0 if no frame captured
char* _ext_getCurrentFrame ();
//returns the maximum number of positions available
int _ext_getMaxCaptureObjects();
//returns a CvRect containing the latest capture
CvRect _ext_getLatestCapture(int position);
//returns true if there is allready an object located at position
int _ext_captureObjectPositionTaken(int position);
//Returns the raw imagedata (should be rgba from image at position) or 0 if no image captured
char* _ext_getImageData(int position);

