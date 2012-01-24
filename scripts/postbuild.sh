#!/bin/bash
TARGET_DIR=/home/olga/workspace/robot/MainFrame/MainFrame/bin/Debug
cp -Rf /usr/local/share/pocketsphinx/model/hmm/en_US/hub4wsj_sc_8k $TARGET_DIR/
cp -f /home/olga/workspace/robot/resources/sphinx/language.dict $TARGET_DIR/
cp -f /home/olga/workspace/robot/resources/sphinx/language.arpa $TARGET_DIR/
cp -f /home/olga/workspace/robot/resources/language.xml $TARGET_DIR/
scp -r $TARGET_DIR/* olga@nikita:/home/olga/robot/
TARGET_DIR=/home/olga/workspace/robot/native
scp -r $TARGET_DIR/*.c olga@nikita:/home/olga/robot/
scp -r $TARGET_DIR/*.h olga@nikita:/home/olga/robot/
TARGET_DIR=/home/olga/workspace/robot/MainFrame
#scp -r $TARGET_DIR olga@nikita:/home/olga/robot/robot

echo "ALL CLEAR"
#scp -r $TARGET_DIR/*.c olga@nikita:/home/olga/robot/
