;	STOP: sudo pk2cmd -P -R
;	START/INSTALL: sudo pk2cmd -P PIC12f629 -M -F servo.hex -Y -T
;	COMPILE: gpasm -dos -w1 servo.asm -o servo.hex; cat servo.lst | grep Error
;	SIMULATE: gpsim -p12f629 -s servo.cod servo.hex
;
; interrupt:	INTCON 0-5
; akademiker-akassa:	OCR: 5735651261577066398 BANK: 5862-8082 SUMMA: 540
; -""-					OCR: 5735651261577066299 SUMMA: 90
;*************************************************************************************************************
	#include    <12f629.inc>
	__config (_HS_OSC & _WDT_OFF & _PWRTE_ON & _MCLRE_OFF & _CP_OFF )
;_HS_OSC
;_INTRC_OSC_NOCLKOUT
;***** VARIABLE DEFINITIONS

#DEFINE		_servo0Port		0x0			; Servo 1 port
#DEFINE		_servo1Port		0x1			; Servo 2 port
#DEFINE		_servo2Port		0x2			; Servo 3 port
#DEFINE		_servo3Port		0x4			; Servo 4 port


#DEFINE		_portData	0x3				; The data input port

cblock 0x20		; Start of memory block
	dc1			; delay counter inner loop
	dc2			; delay counter outer loop
	servo0I		; The current rotation (interval) for servo0
	servo1I		; The current rotation (interval) for servo1
	servo2I		; The current rotation (interval) for servo2
	servo3I		; The current rotation (interval) for servo3

	servoSelect ; [0] = 1 if servo0 has signal
				; [1] = 1 if servo1 has signal
				; [2] = 1 if servo2 has signal
				; [3] = 1 if servo3 has signal

	sPauseI		; Servo pause timer (used before PAUSE) equals a delay for sPauseI / 100ms
	portOutTmp	; containting the currently used servo port
	tmp

;************************************************************
;	Signals:
;	A signal is allways 10 bit, where the 2 first represents
;	The servo to use and the remainding 8 represents the value 
;	to be transfered to the servo.
;**************************************************************
	portCounter	; Bit counter for serialized data
	portData	; will be populated with incoming data
	portSelect	; will contain the servo selection

endc

    org 0x0

INIT:
	clrw
	bsf     STATUS,RP0       ; select Register Page 1
	movlw	b'0'
	movwf	servoSelect
	movwf	servo0I
	movwf	servo1I
	movwf	servo2I
	movwf	servo3I
	movwf	TRISIO			 ; set the port configuration	
	bsf		TRISIO,_portData	;set the input port
	bcf     STATUS,RP0       ; back to Register Page 0

;************* Configuration: ******************'

MAIN:
	bcf		GPIO,_servo0Port
	bcf		GPIO,_servo1Port
	bcf		GPIO,_servo2Port
	bcf		GPIO,_servo3Port
	call CHECK_FOR_SIGNAL	

	btfss	servoSelect,_servo0Port	; if not addressing servo0
	call PAUSE_2MS
	btfss	servoSelect,_servo1Port	; if not addressing servo1
	call PAUSE_2MS
	btfss	servoSelect,_servo2Port	; if not addressing servo2
	call PAUSE_2MS
	btfss	servoSelect,_servo3Port	; if not addressing servo3
	call PAUSE_2MS

	btfsc	servoSelect,_servo0Port	;chick if servo0 was selected
	call	SET_SERVO0
	btfsc	servoSelect,_servo1Port	;chick if servo1 was selected
	CALL	SET_SERVO1
	btfsc	servoSelect,_servo2Port	;chick if servo1 was selected
	CALL	SET_SERVO2
	btfsc	servoSelect,_servo3Port	;chick if servo1 was selected
	CALL	SET_SERVO3

	goto MAIN


;***********************  
;	SET_SERVO methods
; 	sends signal to servo1
;***********************
SET_SERVO0:
	movlw	0x0
	movwf	portOutTmp
	bsf		portOutTmp,_servo0Port
	movfw	servo0I
	call	SET_SERVO
	return

SET_SERVO1:
	movlw	0x0
	movwf	portOutTmp
	bsf		portOutTmp,_servo1Port
	movfw	servo1I
	call	SET_SERVO
	return

SET_SERVO2:
	movlw	0x0
	movwf	portOutTmp
	bsf		portOutTmp,_servo2Port
	movfw	servo2I
	call	SET_SERVO
	return

SET_SERVO3:
	movlw	0x0
	movwf	portOutTmp
	bsf		portOutTmp,_servo3Port
	movfw	servo3I
	call	SET_SERVO
	return



SET_SERVO:
	movwf	sPauseI
	movfw	portOutTmp			; use portOutTmp to select port
	movwf	GPIO				; turn the selected port on
	call	PAUSE				; pause for sPauseI time
	movlw	0x0					
	movwf	GPIO				; turn port off again
	return

;***********************  
;	PAUSE
; 	Pause for 850 us + sPauseI * 7 us
;***********************

PAUSE:
	call PAUSE_50US
	call PAUSE_200US
	call PAUSE_200US
	movfw	sPauseI			; 1		Set dc2 to pauceT (the number of 100'ths of milliseconds to delay)
	movwf	dc2				; 1		
PAUSE_LOOP:
	nop						; 1
	nop						; 1
	nop						; 1
	nop						; 1 
	nop						; 1
	nop						; 1  
	nop						; 1
	nop						; 1  
	decfsz	dc2,1			; 1 (2) counter for inner loop
	goto PAUSE_LOOP 		; 2
	return					; 2		return from subroutine if dc2 == 0


;*****************************************************************************************************************************************************************
;	
;			DATA TRANSFER METHODS:
;
;*****************************************************************************************************************************************************************



;***********************  
;	CHECK_FOR_SIGNAL
; 	Check if data is being transfered. The first signal is allways a "wake up" signal
;***********************
CHECK_FOR_SIGNAL:
	btfsc	GPIO, _portData
;	bsf 	GPIO, _servo0Port
	call	START_TRANSFER
	return

;***********************  
;	SIGNAL_RECEIVED
; 	Handles incoming dataSignal
;***********************
START_TRANSFER:

	clrf	portCounter		;clear the portCounter
	clrf	portData
	clrf	portSelect

TRANSFER_INIT_LOOP:
	btfsc	GPIO, _portData 		; if port data is still set
	goto	TRANSFER_INIT_LOOP		; wait
	call 	PAUSE_50US				; pause to get "out of sync"
	call	SERVO_SELECT_TRANSFER
	call	DATA_TRANSFER

	return

;***********************  
;	SERVO_SELECT_TRANSFER
; 	Initialize serialized data transfer to select servo
;***********************
SERVO_SELECT_TRANSFER:
	movlw	b'0'
	movwf	portSelect
	btfsc	GPIO,_portData	; if signal is received
	bsf		portSelect,_servo0Port	; set the port for servo 0 
	call TRANSFER_DELAY		; wait for next signal

	btfsc	GPIO,_portData	; if signal is received
	bsf		portSelect,_servo1Port	; set the port for servo 1
	call TRANSFER_DELAY		; wait for next signal

	btfsc	GPIO,_portData	; if signal is received
	bsf		portSelect,_servo2Port	; set the port for servo 2 
	call TRANSFER_DELAY		; wait for next signal

	btfsc	GPIO,_portData	; if signal is received
	bsf		portSelect,_servo3Port	; set the port for servo 3
	call TRANSFER_DELAY		; wait for next signal
	return

;***********************  
;	DATA_TRANSFER
; 	Initialize serialized data transfer containing interval
;***********************
DATA_TRANSFER:
	movlw	b'0'
	movwf	portCounter
	movwf	portData
	bsf		portCounter,0x0	;init the dataCounter to the first bit
DATA_TRANSFER_LOOP:
	movf	portCounter,w	;move counter to w
	btfsc	GPIO,_portData	;if signal is received
	iorwf	portData,1		;OR the portCounter to portData
	bcf		STATUS,0		;clear the carry-bit
	rlf		portCounter,1		;rotate portCounter left
	
	btfsc	STATUS,0x0			; if carry is set: finish transfer (full rotation)
	goto 	DATA_TRANSFER_FINISHED
	call 	TRANSFER_DELAY			; wait for next signal
	goto	DATA_TRANSFER_LOOP			; fetch next bit	
DATA_TRANSFER_FINISHED:
	movfw	portData
	btfsc	portSelect,_servo0Port	;chick if servo0 was selected
	movwf	servo0I					;copy to servo
	btfsc	portSelect,_servo1Port	;chick if servo1 was selected
	movwf	servo1I					;copy to servo
	btfsc	portSelect,_servo2Port	;chick if servo1 was selected
	movwf	servo2I					;copy to servo
	btfsc	portSelect,_servo3Port	;chick if servo1 was selected
	movwf	servo3I					;copy to servo
	movfw	portSelect
	iorwf	servoSelect,1			;merge the portSelect and the servoSelect settings
	return

TMPTEST:
	movfw	sPauseI
	movwf	portData
TRANSFER_DATA:
	movfw	portData
	movwf	tmp				; move portData to a temporary file
	movlw	b'00000001'
	movwf	portCounter		; make sure the counter is 1
TL_DATA:
	bcf		STATUS,0		; clear the carry-bit
	rlf		tmp				; rotate right
	btfsc	STATUS,0x0		; if carry is set (last rotation was 1)
	call 	TURN_PORT_ON	; send signal
	call 	PAUSE_45US		; use a slighly shorter delay...
	call 	PAUSE_45US		; ... in order to compensate for the size of this method
	call 	TURN_PORT_OFF
	bcf		STATUS,0		; clear the carry-bit
	rlf		portCounter,1	; rotate portCounter left
	btfsc	STATUS,0x0		; if carry is set: finish transfer (full rotation)
	goto 	TRANSFER_DONE
	goto 	TL_DATA
TRANSFER_DONE:
	goto TRANSFER_DATA	;tmp
return

;***********************  
;	PAUSE SIGNALS
; 	Pauses for the time required to receive a new signal
;	Optimized for a 4 MHz HC49 low profile crystal and 
;	2 x 22 pF ceramic capacitors
;***********************

;***********************  
;	PAUSE_5MS
; 	Pauses for 5 milliseconds. 
;***********************
PAUSE_5MS:
	movlw	d'5'
	movwf	dc2
PAUSE_OUTER_LOOP:
	movlw	d'250'
	movwf	dc1
PAUSE_1MS_INNER_LOOP1:			;pauses for ~ 1 ms
	nop
	decfsz	dc1,1
	goto PAUSE_1MS_INNER_LOOP1
	decfsz	dc2,1
	goto PAUSE_OUTER_LOOP
	return

;***********************  
;	PAUSE_2MS
; 	Pauses for 2 milliseconds. 
;***********************
PAUSE_2MS:
	movlw	d'2'
	movwf	dc2
PAUSE_OUTER_LOOP_2MS:
	movlw	d'250'
	movwf	dc1
PAUSE_1MS_INNER_LOOP1_FOR_2MS:			;pauses for ~ 1 ms
	nop
	decfsz	dc1,1
	goto PAUSE_1MS_INNER_LOOP1_FOR_2MS
	decfsz	dc2,1
	goto PAUSE_OUTER_LOOP_2MS
	return

PAUSE_850US:
	call PAUSE_200US	
	call PAUSE_200US
	call PAUSE_200US
	call PAUSE_200US
	call PAUSE_50US
	return;

PAUSE_200US:
	call PAUSE_50US
	call PAUSE_50US
	call PAUSE_50US
	call PAUSE_50US
	return

PAUSE_50US:
	movlw	d'10'
	movwf	dc1
PAUSE_50US_LOOP:
	nop
	decfsz	dc1,1
	goto PAUSE_50US_LOOP
	return

PAUSE_45US:
	movlw	d'9'
	movwf	dc1
PAUSE_45US_LOOP:
	nop
	decfsz	dc1,1
	goto PAUSE_45US_LOOP
	return


TRANSFER_DELAY:
	;call PAUSE_10MS
	call PAUSE_50US
	call PAUSE_50US
	return

;****************************** PORT METHODS **************************

TURN_PORT_ON:
	movfw	portOutTmp			; use portOutTmp to select port
	movwf	GPIO				; turn the selected port on
return

TURN_PORT_OFF:
	movlw	0x0
	movwf	GPIO
return

end
