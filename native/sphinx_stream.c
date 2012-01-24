#include "sphinx_stream.h"
#include <string.h>

#define MODELDIR "./language"
#define NATIVE_MODELDIR "/usr/local/share/pocketsphinx/model"


static const char*(*text_received)(const char *message);				//delegate method for sending messages
static const char*(*report_error)(int type, const char *message);	//delegate method for reporting back error

static bool is_active = true;

static GMainLoop *loop;

static GstElement *bin, 	// the containing all the elements
		*pipeline, 	 		// the pipeline for the bin
		*alsa_src, 	 		// the microphone input source
		*audio_convert, 	// there should always be audioconvert and audioresample elements before the audio sink...
		*audio_resample, 	// ...since the capabilities of the audio sink usually vary depending on the environment (output used, sound card, driver etc.)
		*vader,				// for threasholding the input to the pocketsphinx
		*asr,				// the main asr (speech to text) engine
		*fakesink;			// a working pipe must have a source (in this case the microphone) and a sink. this case: using a dummy sink.

static GstBus *bus;	//the bus element te transport messages from/to the pipeline

static const char *lm_file_name, *dic_file_name, *hmm_file_name;

//the send error method WILL send errors back to the caller
int send_error (int error_type, const char *error_message)
{
	if (report_error != NULL)
		report_error(error_type, error_message);
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
		g_main_loop_quit(loop);
		break;
	}
	case GST_MESSAGE_ERROR: {
		GError *err;
		gst_message_parse_error(msg, &err, NULL);
		//report error
		bool tmp = send_error(ASR_ERROR_GST, err->message);
		g_error_free(err);
		
		g_main_loop_quit(loop);
		
		break;
	} //indicating an element specific message
	case GST_MESSAGE_APPLICATION: {
		const GstStructure *str;
		str = gst_message_get_structure (msg);
			if (gst_structure_has_name(str,"partial_result"));
				//TODO: do something on partial results?
			else if (gst_structure_has_name(str,"result") && text_received != NULL)
			{
				//trigger the text_received method with the hypothesis as input
				text_received (gst_structure_get_string(str,"hyp"));
								
			}
			else if (gst_structure_has_name(str,"turn_off"))
			{
				g_main_loop_quit(loop);
				return false;
			}
		break;
	}
	default:
	
		break;
	}

		//printf("info: %i %s\n", (int)(msg->timestamp), GST_MESSAGE_TYPE_NAME (msg));

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
	
	//g_main_loop_quit(loop);
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

/*	The initElements configures all the elements of the pipeline, as well as the pipline
	and the bin it self.
	
	it also configures the element/pipeline bus message methods
*/
int init_elements (const char *lm_file, const char *dict_file, const char *hmm_file)
{

	pipeline = gst_pipeline_new ("asr_pipeline");
	bin = gst_bin_new ("asr_bin");

	//initializing elements
	alsa_src = gst_element_factory_make ("alsasrc", "alsa_src");
	audio_convert = gst_element_factory_make ("audioconvert", "audio_convert");
	audio_resample = gst_element_factory_make ("audioresample", "audio_resample");
	vader = gst_element_factory_make ("vader", "vader");
	asr = gst_element_factory_make ("pocketsphinx", "asr");
	fakesink = gst_element_factory_make ("fakesink", "fakesink");

	//check for successfull creation of elements
	if(!alsa_src)
		return send_error(ASR_ERROR_INIT_ALSA_FAILED, "Unable create alsa src (microphone input) object!");
	if(!audio_convert || !audio_resample)
		return send_error(ASR_ERROR_INIT_CONVERTER_FAILED, "Unable create converter/resampler.");
	if(!vader)
		return send_error(ASR_ERROR_INIT_VADER_FAILED, "Unable create vader.");
	if(!asr)
		return send_error(ASR_ERROR_INIT_POCKETSPHINX_FAILED, "Unable create pocketsphinx element. Is the gstpocketsphinx installed?");
	if(!fakesink)
		return send_error(ASR_ERROR_INIT_SINK_FAILED, "Unable create fakesink... strange.");

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

	//add the bus message methods to the asr (complete & partial)
	g_signal_connect (asr, "partial_result", G_CALLBACK (asr_partial_result), NULL);
	g_signal_connect (asr, "result", G_CALLBACK (asr_result), NULL);


	// create the bus for the pipeline:
	bus = gst_pipeline_get_bus(GST_PIPELINE(pipeline));
	//add the bus handler method:
	gst_bus_add_watch(bus, bus_call, NULL);

	gst_object_unref(bus);

	// Add the elements to the bin
	gst_bin_add_many (GST_BIN (bin), 
		alsa_src, 
		audio_convert, 
		audio_resample,  
		vader,
		asr,
		fakesink,
	NULL);

	// add the bin to the pipeline 
	gst_bin_add (GST_BIN (pipeline), bin);

	// link the elements and check for success
	if (!gst_element_link_many (alsa_src, 
		audio_convert, 
		audio_resample,  
		vader,
		asr,
		fakesink,
	NULL))
		return send_error(ASR_ERROR_LINK_FAILED, "Unable to link elements!");

	//creation successfull
	return 0;
}

int asr_start ()
{
	//try to create the elements and initialize them
	if (init_elements(lm_file_name , dic_file_name, hmm_file_name) != 0)
		return 1;

	// create the main loop
	loop = g_main_loop_new(NULL, FALSE);

	//travers the pipe to "play state"
	gst_element_set_state(GST_ELEMENT(pipeline), GST_STATE_PLAYING);

	//start the main loop
	g_main_loop_run(loop);

	//unref elements
	gst_element_set_state(GST_ELEMENT(pipeline), GST_STATE_NULL);
	gst_object_unref(GST_OBJECT(pipeline));
	g_main_loop_unref (loop);
	
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
			MODELDIR "/4734.lm", 
			MODELDIR "/4734.dic",
			NATIVE_MODELDIR "/hmm/en_US/hub4wsj_sc_8k" ) == 0)
		asr_start ();
	else
	{
		printf("Init failed...");
		return -1;
	}

	return 0;
}
