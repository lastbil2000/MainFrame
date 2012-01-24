#include "facedetect.h"
//TODO: store last face for future face recognition

/// static declarations:
static const char*(*haar_detected)(int x, int y, int width, int height);
//static const char*(*face_detected)(const char *message);

static CvCapture* capture = 0;
static int maxHeight, maxWidth;
const char* cascadeFile = 0;
CvHaarClassifierCascade* cascade;
CvMemStorage* storage;
static int shouldRun;
static CvRect haar_object;
static int scale = 2;
/// body:


int init(
		//const char*(*face_detected_callback)(const char *message),
		const char*(*haar_detected_callback)(int x, int y, int width, int height), 
		const char* cascadeFileName, 
		int cameraNO ) {
//	printf ("yes box");
	
	haar_detected = haar_detected_callback;

	//face_detected ("JESUS");
	
	if (cascadeFileName != NULL)
		cascadeFile = cascadeFileName;
	
	capture = cvCaptureFromCAM( cameraNO );

    if( !capture || capture == 0)
    {
        fprintf(stderr,"Could not initialize capturing...\n");
        return -1;
    }

//	printf (" capture =  %i", capture); 

	FILE *fh;
	
	fh = fopen(cascadeFile, "rb");
	if (fh == NULL)
	{
		fprintf(stderr,"Could not open '%s' haar file\n", cascadeFile);
		return -1;
	}
	fclose(fh);

	cascade = (CvHaarClassifierCascade*)cvLoad(cascadeFile, NULL, NULL, NULL);

	storage = cvCreateMemStorage(0);

	return 0;
}




CvRect try_detect_object (IplImage* image) {

	int scale = 2;
	//Scale down the searchable image to half the size:
	IplImage *small_image = cvCreateImage(cvSize(image->width/scale,image->height/scale), IPL_DEPTH_8U, 3);
	cvPyrDown(image, small_image, CV_GAUSSIAN_5x5);
	
	//Detect faces (currently, only one face is detected at best...
	CvSeq* haar_objects = cvHaarDetectObjects(
		small_image, 
		cascade, 
		storage, 
		1.2f, 
		2, 
		CV_HAAR_DO_CANNY_PRUNING | CV_HAAR_FIND_BIGGEST_OBJECT | CV_HAAR_DO_ROUGH_SEARCH, 
		cvSize(0,0)
	);
	
	cvReleaseImage(&small_image);
	
	//currently only checking the first element
	if (haar_objects->total > 0)
	{
		//CvRect face = *(CvRect*)cvGetSeqElem(haar_objects, 0);
		//printf ("ARNOLD %i %i ", face.x, face.y);
		return *(CvRect*)cvGetSeqElem(haar_objects, 0);
		
		
		//face_detected ("ARNOLD!");
	}
	
	return cvRect(0,0,0,0);


}



void start () {
	
	shouldRun = 1;

	if (capture == 0)
	{
		fprintf(stderr,"Could start. No capture device specified (have you initialized through 'init'?)\n");
	}

	//loop and capture from device
	for(;;)
    {
		
		if (shouldRun == 0)
			break;
		
        IplImage* frame = 0;

		frame = cvQueryFrame( capture );

        if( !frame ) {
			fprintf(stderr,"Unable to capture frame from camera.\n");
            break;
		}
		maxWidth = frame->width;
		maxHeight = frame->height;
		haar_object = try_detect_object (frame);
		
		if ( (haar_object.x != 0) && 
			 (haar_object.y != 0) &&
			 (haar_object.width != 0) &&
			 (haar_object.height != 0))
				haar_detected (haar_object.x, haar_object.y, haar_object.width, haar_object.height);
		
		

	}
}

void stop () {
	shouldRun = 0;
	cvReleaseMemStorage(&storage);
	//cvReleaseHaarClassifierCascade(&cascade);
}
int getMaxWidth() {
	return maxWidth;
}

int getMaxHeight() {
	return maxHeight;
}

