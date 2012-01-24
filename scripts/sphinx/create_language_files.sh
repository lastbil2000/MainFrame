#!/bin/bash

BASE_DIR=$1
LANGUAGE_BASE_NAME=$2
TARGET_DIR=$3

if [ ! -e $BASE_DIR ]; then
	echo "$BASE_DIR does not exist!"
	exit -1
fi

if [ -e $LANGUAGE_BASE_NAME ]; then
	echo "argument 2 - language base name - is not set"
	exit -1
fi;


SPHINX_DIR=$BASE_DIR/resources/sphinx
CORPUS_FILE_NAME=$LANGUAGE_BASE_NAME.corpus
MARKUP_FILE_NAME=$LANGUAGE_BASE_NAME.markup


cd $BASE_DIR/scripts/sphinx

#compile the $LANGUAGE_BASE_NAME.xml into files used to create sphinx resources (below)
#-import /usr/local/share/pocketsphinx/model/lm/en_US/cmu07a.dic
#$SPHINX_DIR/cmu07a.dic.test
./LanguageParser.exe -s $BASE_DIR/resources/$LANGUAGE_BASE_NAME.xml -lm $SPHINX_DIR/$MARKUP_FILE_NAME -dict $SPHINX_DIR/$CORPUS_FILE_NAME 
echo "... done"
echo ""

#exit 0

#create a language model file (LM), a pre-compiled language model file (DMP) and other stuff...
cd $SPHINX_DIR
rm -R cmuclmtk-*
echo ""
echo "--------------text2wfreq-----------------"
text2wfreq < $MARKUP_FILE_NAME | wfreq2vocab > $LANGUAGE_BASE_NAME.vocab
echo ""
echo "--------------text2idngram-----------------"
text2idngram -vocab $LANGUAGE_BASE_NAME.vocab -idngram $LANGUAGE_BASE_NAME.idngram < $MARKUP_FILE_NAME
#text2idngram -vocab $LANGUAGE_BASE_NAME.vocab -idngram $LANGUAGE_BASE_NAME.idngram -write_ascii < $MARKUP_FILE_NAME > $LANGUAGE_BASE_NAME.idngram

echo ""
echo "--------------idngram2lm-----------------"
idngram2lm -vocab_type 0 -context $LANGUAGE_BASE_NAME.ccs -idngram $LANGUAGE_BASE_NAME.idngram -vocab $LANGUAGE_BASE_NAME.vocab -arpa $LANGUAGE_BASE_NAME.unsorted.arpa -good_turing -disc_ranges 0 0 0
#idngram2lm  -idngram $LANGUAGE_BASE_NAME.idngram -vocab $LANGUAGE_BASE_NAME.vocab  -arpa $LANGUAGE_BASE_NAME.unsorted.arpa -context $LANGUAGE_BASE_NAME.ccs -vocab_type 0 -good_turing -disc_ranges 0 0 0 -ascii_input

#OLD: sphinx_lm_convert -i $LANGUAGE_BASE_NAME.arpa -o $LANGUAGE_BASE_NAME.lm.DMP
echo ""
echo "---------------sphinx_lm_sort----------------"
sphinx_lm_sort < $LANGUAGE_BASE_NAME.unsorted.arpa > $LANGUAGE_BASE_NAME.arpa
echo ""
echo "-----------------sphinx_lm_convert--------------"
sphinx_lm_convert -i $LANGUAGE_BASE_NAME.arpa -o $LANGUAGE_BASE_NAME.dmp
#sphinx_lm_convert -i $LANGUAGE_BASE_NAME.dmp -ifmt dmp -o $LANGUAGE_BASE_NAMEl.arpa -ofmt arpa

echo "... done"
echo "-----------------make_pronunciation.pl--------------"
#create the dictionary out of the un-compiled lm-file
cd $BASE_DIR/scripts/sphinx/MakeDict
perl make_pronunciation.pl -tools .. -dictdir $SPHINX_DIR -words $CORPUS_FILE_NAME -handdict $LANGUAGE_BASE_NAME.hand.dict -dict $LANGUAGE_BASE_NAME.dict

echo ""
echo "copying files..."

cp -Rf /usr/local/share/pocketsphinx/model/hmm/en_US/hub4wsj_sc_8k $TARGET_DIR/
cp -f /home/olga/workspace/robot/resources/sphinx/language.dict $TARGET_DIR/
cp -f /home/olga/workspace/robot/resources/sphinx/language.arpa $TARGET_DIR/
#cp -f /home/olga/workspace/robot/resources/language.xml $TARGET_DIR/
