//46 & 75 i KeyboardProcess - hur vet den "robot"?
#include "video.h"
#define MAX_CAPTURE_OBJECTS 10
static CvCapture* capture = 0;
static int screenHeight, screenWidth;
static int shouldRun;
static int scale = 2;
//contains the current frame:
static IplImage* currentFrame = 0;
static IplImage *currentFrameRGBA = 0;

static CaptureObject captureObjects[MAX_CAPTURE_OBJECTS];

/// body:

CvRect zeroRect() {
	return cvRect(0,0,0,0);
}

int _ext_getMaxCaptureObjects() {
	return MAX_CAPTURE_OBJECTS;
}

int _ext_captureObjectPositionTaken(int position) {
	return captureObjects[position].initialized;
}

#define VIDEO_ERR_OUT_OF_BOUNDS 1
#define VIDEO_ERR_NO_FILES_SPECIFIED 2
#define VIDEO_ERR_UNABLE_TO_OPEN_HAAR 4
#define VIDEO_ERR_CAPTURES_IS_FULL 8

int _ext_createCaptureObject(int position, const char* cascadeFileName) {
	
	if (position < 0 || position >= MAX_CAPTURE_OBJECTS) {
		fprintf(stderr, "Position (%i) is out of bounds (max: %i)", position, MAX_CAPTURE_OBJECTS - 1);
		return VIDEO_ERR_OUT_OF_BOUNDS  ;			
	}
	
	if (cascadeFileName == NULL) {
		fprintf(stderr, "No cascade file specified");
		return VIDEO_ERR_NO_FILES_SPECIFIED;	
	}
	FILE *fh;
	
	fh = fopen(cascadeFileName, "rb");
	if (fh == NULL)
	{
		fprintf(stderr,"Could not open '%s' haar file\n", cascadeFileName);
		return VIDEO_ERR_UNABLE_TO_OPEN_HAAR;
	}

	fclose(fh);
	
	if (captureObjects[position].initialized) {
		fprintf(stderr, "position for object to capture (%i) already occupied by %s.", 
			position, 
			captureObjects[position].cascadeFileName);
		return VIDEO_ERR_CAPTURES_IS_FULL;
	}

	captureObjects[position].cascadeFileName = cascadeFileName;
	captureObjects[position].cascade = (CvHaarClassifierCascade*)cvLoad(cascadeFileName, NULL, NULL, NULL);
	captureObjects[position].initialized = 1;
	captureObjects[position].storage = cvCreateMemStorage(0);
	captureObjects[position].hasCapturedImage = 0;
	captureObjects[position].capturedImage = 0;


	return 0;
	
}

void releaseCaptureObject(int position) {
	if (captureObjects[position].initialized) {
		cvReleaseHaarClassifierCascade(&captureObjects[position].cascade);
		cvReleaseMemStorage(&captureObjects[position].storage);
	}

	if (captureObjects[position].hasCapturedImage) {
		cvReleaseImage(&(captureObjects[position].capturedImage));
	}

	captureObjects[position].hasCapturedImage = 0;
	captureObjects[position].capturedImage = 0;
	captureObjects[position].cascadeFileName = 0;
	captureObjects[position].cascade = 0;
	captureObjects[position].initialized = 0;
	captureObjects[position].didCapture = 0;
	captureObjects[position].latestCapture = zeroRect();
	captureObjects[position].bounds = zeroRect();
	
}

int isZeroRect(CvRect rect) {
	return 
		(rect.x == 0) && 
		(rect.y == 0) &&
		(rect.width == 0) &&
		(rect.height == 0);
}

int _ext_init(
		int cameraNO ) {

	capture = cvCaptureFromCAM( cameraNO );

    if( !capture || capture == 0)
    {
        fprintf(stderr,"Could not initialize capturing...\n");
        return -1;
    }
	/*
	for (int i = 0; i < MAX_CAPTURE_OBJECTS; i++ ) 
		releaseCaptureObject(i);
	*/
	return 0;
}



void _ext_setCaptureObjectBounds(int position, CvRect bounds) {
	captureObjects[position].bounds = bounds;
}

void _ext_releaseCaptureObjectBounds(int position) {
	captureObjects[position].bounds = zeroRect();
}


CvRect tryDetectObject (IplImage* image, int position) {

	if (image == 0) {
		fprintf(stderr, "no frame detected when capturing for object: %i\n", position);
		return zeroRect();
	}

	int scale = 2;
	//Scale down the searchable image to half the size:
	IplImage *small_image = cvCreateImage(cvSize(image->width/scale,image->height/scale), IPL_DEPTH_8U, 3);
	cvPyrDown(image, small_image, CV_GAUSSIAN_5x5);
//	return zeroRect();

	
	//Detects one object. yay!
	CvSeq* haar_objects = cvHaarDetectObjects(
		small_image, 
		captureObjects[position].cascade, 
		captureObjects[position].storage,
		1.2f, 
		2, 
		CV_HAAR_DO_CANNY_PRUNING | CV_HAAR_FIND_BIGGEST_OBJECT | CV_HAAR_DO_ROUGH_SEARCH, 
		cvSize(0,0),
		cvSize(200,200)
	);
	
	cvReleaseImage(&small_image);

	//currently only checking the first element
	if (haar_objects->total > 0)
	{
		//copy image
		
		CvRect result = *(CvRect*)cvGetSeqElem(haar_objects, 0);
		result.x *= scale;
		result.y *= scale;
		result.width *= scale;
		result.height *= scale;
		captureObjects[position].capturedImageWidth = result.width;
		captureObjects[position].capturedImageWidth = result.height;

		if (captureObjects[position].hasCapturedImage) {
			cvReleaseImage(&(captureObjects[position].capturedImage));
			captureObjects[position].hasCapturedImage = 0;
			captureObjects[position].capturedImage = 0;
		}

		captureObjects[position].capturedImage = cvCreateImage(cvSize(result.width, result.height), IPL_DEPTH_8U, 4);

		cvSetImageROI(image, result);
		cvCvtColor(image, captureObjects[position].capturedImage, CV_RGB2RGBA);
		//cvCopy(image, captureObjects[position].capturedImage, NULL);

		captureObjects[position].hasCapturedImage = 1;
		return result;
	}
	
	
	//no object found
	return zeroRect();
}

int _ext_didCapture(int position) {
	return captureObjects[position].didCapture;
}

int do_capture (int position) {
	if (currentFrame == 0)
		return 0;

	if (!isZeroRect(captureObjects[position].bounds)) {
		cvSetImageROI(currentFrame, captureObjects[position].bounds);
	}
	IplImage *image = cvCreateImage(cvGetSize(currentFrame),
									currentFrame->depth,
									currentFrame->nChannels);
	cvCopy (currentFrame, image, NULL);

	if (!isZeroRect(captureObjects[position].bounds)) {
		cvResetImageROI(currentFrame);
	}

	CvRect result = tryDetectObject(image, position);

	cvReleaseImage(&image);	

	//fprintf(stderr, "FOUND: %i %i %i %i\n", result.x, result.y, result.width, result.height);	
	if (isZeroRect(result))
		return 0;

	captureObjects[position].latestCapture = result;
	captureObjects[position].latestCapture.x += captureObjects[position].bounds.x;
	captureObjects[position].latestCapture.y += captureObjects[position].bounds.y;

	return 1;

}

void _ext_capture(int position) {
	captureObjects[position].shouldCapture = 1;
}


CvRect _ext_getLatestCapture(int position) {
	return captureObjects[position].latestCapture;
}

int _ext_isCapturing(int position) {
	return captureObjects[position].shouldCapture;
}

int _ext_start () {
	
	shouldRun = 1;

	if (capture == 0)
	{
		fprintf(stderr,"Could start. No capture device specified (have you initialized through 'init'?)\n");
	}

	//loop and capture from device
	//for(;;) {
		
		if (shouldRun != 1)
			return 0;

		currentFrame = cvQueryFrame( capture );

        if( !currentFrame ) {
			fprintf(stderr,"Unable to capture frame from camera.\n");
			shouldRun = -1;
            return -1;
		}

		screenWidth = currentFrame->width;
		screenHeight = currentFrame->height;

		for (int i = 0; i < MAX_CAPTURE_OBJECTS; i++ ) {
			if (captureObjects[i].shouldCapture) {
				captureObjects[i].didCapture = do_capture(i);
			}
			captureObjects[i].shouldCapture = 0;
		}

	//}


//	if (currentFrame != 0)
//		cvReleaseImage(&currentFrame);

	return 0;
}

void _ext_stop () {
	shouldRun = 0;

	for (int i = 0; i < MAX_CAPTURE_OBJECTS; i++ ) 
		releaseCaptureObject(i);

	if (currentFrameRGBA != 0)
		cvReleaseImage(&currentFrameRGBA);
}


char* _ext_getCurrentFrame () {

	if (currentFrame != 0)
	{

		currentFrameRGBA = cvCreateImage(cvSize(currentFrame->width, currentFrame->height), IPL_DEPTH_8U, 4);
		cvCvtColor(currentFrame, currentFrameRGBA, CV_RGB2RGBA);
		//printf("row widthStep: %i screenWidth * 3: %i \n", currentFrameRGBA->widthStep, screenWidth * 4);
		
		return currentFrameRGBA->imageData;
	}

	return 0;
}

//Returns the raw imagedata (should be rgba from image at position
char* _ext_getImageData(int position) {

	if (captureObjects[position].hasCapturedImage) {

		return captureObjects[position].capturedImage->imageData;
	}

	return 0;
}

int _ext_getScreenWidth() {
	return screenWidth;
}

int _ext_getScreenHeight() {
	return screenHeight;
}

