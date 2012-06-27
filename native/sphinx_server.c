#include "sphinx_server.h"
#include <string.h>

//#define MODELDIR "./language"
//#define MODELDIR "/home/olga/workspace/robot/native/language"
#define MODELDIR ""
#define NATIVE_MODELDIR "/usr/local/share/pocketsphinx/model"


static const char*(*text_received)(const char *message);				//delegate method for sending messages
static const char*(*report_error)(int type, const char *message);	//delegate method for reporting back error

static bool is_active = true;
static float _amplification = 2.0;
static float _cutoff = 4000.0;

static GMainLoop *loop;

static GstElement 
		//*bin, 	// the containing all the elements
		*pipeline, 	 		// the pipeline for the bin
		*alsa_src, 	 		// the microphone input source
		*audio_resample, 	// ...since the capabilities of the audio sink usually vary depending on the environment (output used, sound card, driver etc.)
		*vader,				// for threasholding the input to the pocketsphinx
		*asr,				// the main asr (speech to text) engine
		*amplifier,			// audioamplify
		*filter,			// audioiirfilter for white noise reduction
		*conv0, 			// audioconvert0
		*conv1,				// audioconvert1
		*conv2,				// audioconvert2
		*conv3,				// audioconvert3
		*fakesink;			// a working pipe must have a source (in this case the microphone) and a sink. this case: using a dummy sink.

static GstElement
		*filesrc, 
		*demuxer,
		*faad,
		*faac,
		*decodebin,
		*autoaudiosink;
static GstBus *bus;	//the bus element te transport messages from/to the pipeline

static const char *lm_file_name, *dic_file_name, *hmm_file_name;


//dealocates and shuts down
void turn_off () {
	g_main_loop_quit(loop);
	gst_element_set_state(GST_ELEMENT(pipeline), GST_STATE_NULL);
	gst_object_unref(GST_OBJECT(pipeline));
	g_main_loop_unref (loop);
}

//the send error method WILL send errors back to the caller
int send_error (int error_type, const char *error_message)
{
	if (report_error != NULL) {
		report_error(error_type, error_message);
	}
	printf ("-------------------------- ERROR: '%s', code: %i\n", error_message, error_type);
	return true;
}

char *strdupa (const char *s) {
    char *d = malloc (strlen (s) + 1);   // Allocate memory
    if (d != NULL)
        strcpy (d,s);                    // Copy string if okay
    return d;                            // Return new memory
}

static gboolean bus_call(GstBus *bus, GstMessage *msg, void *user_data)
{

	switch (GST_MESSAGE_TYPE(msg)) {
	case GST_MESSAGE_EOS: {
		g_message("End-of-stream");
		//report the end of stream
		bool tmp = send_error(ASR_EOS, "Unexpected end of stream");
		turn_off();
		break;
	}
	case GST_MESSAGE_ERROR: {
		GError *err;
		gst_message_parse_error(msg, &err, NULL);
		//report error
		bool tmp = send_error(ASR_ERROR_GST, err->message);
		g_error_free(err);
		
		//turn_off();
		
		break;
	} //indicating an element specific message
	case GST_MESSAGE_APPLICATION: {
		const GstStructure *str;
		str = gst_message_get_structure (msg);
			if (gst_structure_has_name(str,"partial_result"));
				//TODO: do something on partial results?
			else if (gst_structure_has_name(str,"result"))
			{
				//trigger the text_received method with the hypothesis as input
				if (text_received != NULL)
					text_received (gst_structure_get_string(str,"hyp"));
				else
					printf ("WARNING: (text_received not set): %s\n", gst_structure_get_string(str,"hyp"));
								
			}
			else if (gst_structure_has_name(str,"turn_off"))
			{
				turn_off();
				return false;
			}
		break;
	}
	default:
	
		break;
	}

		//printf("-------------info: %i %s\n", (int)(msg->timestamp), GST_MESSAGE_TYPE_NAME (msg));

	return true;
}

int asr_turn_off()
{
	
	//hmm... there's probably a better way to create a gvalue containing a string from the gchararray...
	GValue text_gv = {0};
	g_value_init (&text_gv, G_TYPE_STRING);
	g_value_set_string(&text_gv, "turn_off");

	//creates message structure for the partial result:
	GstStructure *messageStruct = gst_structure_empty_new ("turn_off");
	//set the hypothesis (the guessed text)
	gst_structure_set_value (messageStruct, "hyp", &text_gv);	

	//post message to the pipeline bus
	
	if (gst_bus_post(
		gst_element_get_bus(asr), 
		gst_message_new_application((GstObject *) asr, messageStruct)))
	{
		//TODO: something if post failed...
	}
		//unref elements
	
	printf("---------------------------------turned off asr engine.");
	return 1;
	
}


//the partial result method is triggered when a comprehensive part of the audio input is transcribed
void asr_partial_result (GstElement* asr,  gchararray text, gchararray uttid, gpointer user_data)
{
	//hmm... there's probably a better way to create a gvalue containing a string from the gchararray...
	GValue text_gv = {0};
	g_value_init (&text_gv, G_TYPE_STRING);
	g_value_set_string(&text_gv, text);

	//creates message structure for the partial result:
	GstStructure *messageStruct = gst_structure_empty_new ("partial_result");
	//set the hypothesis (the guessed text)
	gst_structure_set_value (messageStruct, "hyp", &text_gv);	

	//post message to the pipeline bus
	if (is_active)
		if (gst_bus_post(
			gst_element_get_bus(asr), 
			gst_message_new_application((GstObject *) asr, messageStruct)))
		{
			//TODO: something if post failed...
		}
}

/* the result method is triggered when a full part of the audio input is transcribed */
void asr_result (GstElement* asr,  gchararray text, gchararray uttid, gpointer user_data)
{
	//
	//hmm... there's probably a better way to create a gvalue containing a string from the gchararray...
	GValue text_gv = {0};
	g_value_init (&text_gv, G_TYPE_STRING);
	g_value_set_string(&text_gv, text);

	//creates message structure for the partial result:
	GstStructure *messageStruct = gst_structure_empty_new ("result");
	//set the hypothesis (the guessed text)
	gst_structure_set_value (messageStruct, "hyp", &text_gv);	

	//post message to the pipeline bus
	if (is_active)
		if (gst_bus_post(
			gst_element_get_bus(asr), 
			gst_message_new_application((GstObject *) asr, messageStruct)))
		{
			//TODO: something if post failed...
		}
}

float cutoff(float new_cutoff) {
	if (new_cutoff != 0) {
		_cutoff = new_cutoff;
	}
	return _cutoff;
}
float amplification (float new_amplification) {
	if (new_amplification != 0) {
		_amplification = new_amplification;
		g_object_set (G_OBJECT (amplifier), "amplification", _amplification, NULL);
		g_main_loop_quit(loop);
		gst_element_set_state(GST_ELEMENT(pipeline), GST_STATE_NULL);
		//travers the pipe to "play state"
		gst_element_set_state(GST_ELEMENT(pipeline), GST_STATE_PLAYING);
		//start the main loop
		g_main_loop_run(loop);
	
	}
	return _amplification;
	

}

//white noise detection bus callback
//http://gstreamer.freedesktop.org/data/doc/gstreamer/head/gst-plugins-good-plugins/html/gst-plugins-good-plugins-audioiirfilter.html
static void
on_rate_changed (GstElement * element, gint rate, gpointer user_data)
{
  GValueArray *va;
  GValue v = { 0, };
  gdouble x;

  if (rate / 2.0 > _cutoff)
    x = exp (-2.0 * G_PI * (_cutoff / rate));
  else
    x = 0.0;

  va = g_value_array_new (1);

  g_value_init (&v, G_TYPE_DOUBLE);
  g_value_set_double (&v, 1.0 - x);
  g_value_array_append (va, &v);
  g_value_reset (&v);
  g_object_set (G_OBJECT (element), "a", va, NULL);
  g_value_array_free (va);

  va = g_value_array_new (1);
  g_value_set_double (&v, x);
  g_value_array_append (va, &v);
  g_value_reset (&v);
  g_object_set (G_OBJECT (element), "b", va, NULL);
  g_value_array_free (va);
}

/*	The initElements configures all the elements of the pipeline, as well as the pipline
	and the bin it self.
	
	it also configures the element/pipeline bus message methods
*/

static void
on_pad_added (GstElement *element,
              GstPad     *pad,
              gpointer    data)
{
  GstPad *sinkpad;
  GstElement *decoder = (GstElement *) data;

  /* We can now link this pad with the vorbis-decoder sink pad */
  g_print ("Dynamic pad created, linking demuxer/decoder\n");

  sinkpad = gst_element_get_static_pad (decoder, "sink");

  gst_pad_link (pad, sinkpad);

  gst_object_unref (sinkpad);
}

int init_elements (const char *lm_file, const char *dict_file, const char *hmm_file)
{

	// create the main loop
	loop = g_main_loop_new(NULL, FALSE);

	pipeline = gst_pipeline_new ("asr_pipeline");
	//bin = gst_bin_new ("asr_bin");

	//initializing elements
	alsa_src = gst_element_factory_make ("alsasrc", "alsa_src");
	audio_resample = gst_element_factory_make ("audioresample", "audio_resample");
	vader = gst_element_factory_make ("vader", "vader");
	asr = gst_element_factory_make ("pocketsphinx", "asr");
	fakesink = gst_element_factory_make ("fakesink", "fakesink");
	amplifier = gst_element_factory_make ("audioamplify", "amp");
	filter = gst_element_factory_make ("audioiirfilter", NULL);
	conv0 = gst_element_factory_make ("audioconvert", "audioconvert0");
	conv1 = gst_element_factory_make ("audioconvert", "audioconvert1");
	conv2 = gst_element_factory_make ("audioconvert", "audioconvert2");
	conv3 = gst_element_factory_make ("audioconvert", "audioconvert3");

	//playback test:
	filesrc = gst_element_factory_make ("tcpserversrc", "source"); 
	g_object_set(G_OBJECT(filesrc), "host", "127.0.0.1", NULL);
	g_object_set(G_OBJECT(filesrc), "port", "3000", NULL);

	//filesrc = gst_element_factory_make ("filesrc", "file_src");
	//g_object_set(G_OBJECT(filesrc), "location", "test.m4a", NULL);
	demuxer = gst_element_factory_make ("qtdemux", "demuxer");
	faad = gst_element_factory_make ("faad", "faad");
	autoaudiosink =gst_element_factory_make ("autoaudiosink", "autoaudiosink");
	faac = gst_element_factory_make ("faac", "faac");
	decodebin =gst_element_factory_make ("decodebin", "decodebin");


	if (!filesrc || !demuxer || !faad || !autoaudiosink || !decodebin || !faac)
		return send_error(42, "Unable to create tmp playback test");

	

	//check for successfull creation of elements
	if(!alsa_src)
		return send_error(ASR_ERROR_INIT_ALSA_FAILED, "Unable create alsa src (microphone input) object!");
	if(!audio_resample)
		return send_error(ASR_ERROR_INIT_RESAMPLER_FAILED, "Unable create resampler.");
	if(!vader)
		return send_error(ASR_ERROR_INIT_VADER_FAILED, "Unable create vader.");
	if(!asr)
		return send_error(ASR_ERROR_INIT_POCKETSPHINX_FAILED, "Unable create pocketsphinx element. Is the gstpocketsphinx installed?");
	if(!fakesink)
		return send_error(ASR_ERROR_INIT_SINK_FAILED, "Unable create fakesink... strange.");
	if(!conv0 || !conv1 || !conv2 || !conv3)
		return send_error(ASR_ERROR_INIT_CONVERTER_FAILED, "Unable create converter... strange.");
	if(!filter)
		return send_error(ASR_ERROR_INIT_IIRFILTER_FAILED, "Unable create audioiirfilter (white noise)... strange.");
	if(!amplifier)
		return send_error(ASR_ERROR_INIT_AMPLIFIER_FAILED, "Unable create audioamplify... strange.");

	//set up the vader to auto-threshold:
	g_object_set(G_OBJECT(vader), "auto_threshold", true, NULL);

	//set the directory containing acoustic model parameters
	g_object_set(G_OBJECT(asr), "hmm",  hmm_file , NULL);
	//set the language model of the asr
	g_object_set(G_OBJECT(asr), "lm",  lm_file , NULL);
	//set the dictionary of the asr
	g_object_set(G_OBJECT(asr), "dict",  dict_file , NULL);
	//set the asr to be configured before receiving data
	g_object_set(G_OBJECT(asr), "configured",  true , NULL);
	//set the default amplification
	g_object_set (G_OBJECT (amplifier), "amplification", _amplification, NULL);
	//add the bus message methods to the asr (complete & partial)
	g_signal_connect (asr, "partial_result", G_CALLBACK (asr_partial_result), NULL);
	g_signal_connect (asr, "result", G_CALLBACK (asr_result), NULL);
	//buss call for white noise reduction
	g_signal_connect (G_OBJECT (filter), "rate-changed", G_CALLBACK (on_rate_changed), NULL);

	// create the bus for the pipeline:
	bus = gst_pipeline_get_bus(GST_PIPELINE(pipeline));
	//add the bus handler method:
	gst_bus_add_watch(bus, bus_call, loop);
	//OLD: 	gst_bus_add_watch(bus, bus_call, NULL);
	g_signal_connect (G_OBJECT (bus), "message", G_CALLBACK (bus_call), loop);
	gst_object_unref(bus);

	// Add the elements to the bin
	gst_bin_add_many (GST_BIN (pipeline), 
		filesrc, demuxer, faad,
//a		alsa_src, 
//		filter,
		conv3,
		audio_resample,  
		vader,
//x		conv1,
//x		amplifier,
//x		conv2,
		asr,
		fakesink,
//			autoaudiosink,
	NULL);

	// add the bin to the pipeline 
	//OLD: gst_bin_add (GST_BIN (pipeline), bin);

	//link the source the demuxer:
	gst_element_link (filesrc,demuxer);

	// link the elements and check for success
	if (!gst_element_link_many (
		faad,
//a		alsa_src, 
//		filter,
		conv3,
		audio_resample,  
		vader,
//x		conv1,
//x		amplifier,
//x		conv2,
		asr,
		fakesink,
//			autoaudiosink,
	NULL))
	 	return send_error(ASR_ERROR_LINK_FAILED, "Unable to link elements!");

	//set up the dynamic pad callback for the demuxerer
	g_signal_connect (demuxer, "pad-added", G_CALLBACK (on_pad_added), faad);

	//creation successfull
	return 0;
}

int asr_start ()
{

	//try to create the elements and initialize them
	int init_elements_result = init_elements(lm_file_name , dic_file_name, hmm_file_name);
	if (init_elements_result  != 0) {		
		return 1;
	}
	
	printf ("--------------------- THIS WAS GOOD");		

	//travers the pipe to "play state"
	gst_element_set_state(GST_ELEMENT(pipeline), GST_STATE_PLAYING);

	//start the main loop
	g_main_loop_run(loop);

	
	return 0;
}

void asr_set_text_received_callback(const char*(*text_received_callback)(const char *message ))
{
	text_received = text_received_callback;
}
void asr_set_report_error_callback (const char*(*report_error_callback)(int type, const char *message))
{
	report_error = report_error_callback;
}

int asr_init (const char*(*text_received_callback)(const char *message ),
		  const char*(*report_error_callback)(int type, const char *message),
		  const char *lm_file, const char *dict_file, const char *hmm_file)
{

	asr_set_text_received_callback(text_received_callback);
	asr_set_report_error_callback(report_error_callback);
	
	//check for file existance!%!
	FILE *fh;
	
	fh = fopen(lm_file, "rb");
    if (fh == NULL) {
		printf("Failed to open lm_file:%s", lm_file);
		return 1;
	}
	fclose(fh);

	fh = fopen(dict_file, "rb");
    if (fh == NULL) {
		printf("Failed to open dict_file:%s", dict_file);
		return 1;
	}
	fclose(fh);	
	
	fh = fopen(hmm_file, "rb");
    if (fh == NULL) {
		printf("Failed to open hmm_file:%s", hmm_file);
		return 1;
	}
	fclose(fh);

	
	lm_file_name = strdupa(lm_file); //copy and use stack! (gnu only)
	dic_file_name = strdupa(dict_file);
	hmm_file_name = strdupa(hmm_file);

	//initialize gst(!)
	gst_init (NULL, NULL);

	return 0;

}

void set_is_active (bool report_mode)
{
	is_active = report_mode;
}

bool get_is_active () 
{
	return is_active;
}



int main (int argc, char *argv[])
{

	if (asr_init (0,0,
//			MODELDIR "/4734.lm", 
//			MODELDIR "/4734.dic",
			"language.arpa",
			"language.dict",
			NATIVE_MODELDIR "/hmm/en_US/hub4wsj_sc_8k" ) == 0)
		asr_start ();
	else
	{
		printf("Init failed...");
		return -1;
	}

	return 0;
}
