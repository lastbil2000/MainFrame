;	STOP: sudo pk2cmd -P -R
;	START/INSTALL: sudo pk2cmd -P PIC12f629 -M -F comunit.hex -Y -T
;	COMPILE: gpasm -dos -w1 comunit.asm -o comunit.hex; cat comunit.lst | grep Error
;	SIMULATE: gpsim -p12f629 -s comunit.cod comunit.hex
;
; interrupt:	INTCON 0-5
;*************************************************************************************************************
	#include    <12f629.inc>
	__config (_HS_OSC  & _WDT_OFF  & _MCLRE_OFF & _CP_OFF & _BODEN_ON & _PWRTE_ON )
;_INTRC_OSC_NOCLKOUT
;***** VARIABLE DEFINITIONS

#DEFINE		_portData	0x3				; The input port
#DEFINE		_portStatus	0x2				; The status port used continue transfer (checked after each transfer)
#DEFINE		_portOut0	0x0				; Output 0
#DEFINE		_portOut1	0x1				; Output 1
#DEFINE		_headerBit	0x4				; the header bit (7 means 1 bit header, 0 means 8 bits of header)

#DEFINE		_brownOutIndicatorPort 0x1

cblock 0x20		; Start of memory block
	dc1			; delay counter inner loop
	dc2			; delay counter outer loop

;************************************************************
;	input signals: 2 + 4 + 8 = 15 bit 
;	A signal is allways 2 bit port-select data, 4 bit header data and 8 bit data
;**************************************************************
	portCounter	; Bit counter for serialized data
	portSelect	; will contain the 2 bit output port selection
	portHeader	; will contain the incoming header (currently statically set to 4 bits)
	portData	; will be populated with incoming data

	tmp
endc

    org 0x0

;************* Configuration: ******************'
INIT:
	clrw
	clrf	GPIO
	clrf	TRISIO
	bsf     STATUS,RP0       	; select Register Page 1
 	movlw	b'0'
	movwf	TRISIO
	bsf		TRISIO,_portData	; set _portData as input
;tmp låt status vara utport för lampan:	
	bsf TRISIO,_portStatus	; set _portStatus as input
	bsf		WPU,_portData		; set _portData as weak pul-up
	bsf		WPU,_portStatus		; set _portData as weak pul-up
	bcf     STATUS,RP0       	; back to Register Page 0
	
;************* Brown-out detection: ******************'
;	banksel		PCON
	btfsc		PCON, NOT_POR	; if power-on reset: check if bod
	goto		CHK_BOD
	bsf			PCON,NOT_POR
	bsf			PCON,NOT_BOD

CHK_BOD:
	btfsc		PCON,NOT_BOD		;if no brown-out detected
	goto		MAIN
	bsf			PCON,NOT_BOD
	bsf			GPIO, _brownOutIndicatorPort
;************* Main program: ******************'

MAIN:
MAIN_LOOP:
;	bsf		GPIO,0x1	
;	call PAUSE_10MS	
;	bcf		GPIO,0x1
;	call	LAMPTESTET		;tmp
;	call PAUSE_10MS	
	call CHECK_FOR_SIGNAL
	goto MAIN_LOOP

;*****************************************************************************************************************************************************************
;	
;			DATA TRANSFER METHODS:
;
;			output signal contains of 
;			1 initialization signal (10 ms)
;			4 bit header signals (4 * 100 us)
;			8 bit data signal (8 * 100 us)
;*****************************************************************************************************************************************************************

TRANSFER:
	;call	LAMPTESTET		;tmp
	call TURN_PORT_ON		; say hello (init transfer)
	call PAUSE_10MS			; wait for receiver to accept transfer
	call TURN_PORT_OFF
	
TRANSFER_HEADER:
	btfsc	portHeader,0x4; skip if first header bit is zero
	call	TURN_PORT_ON
	call 	TRANSFER_DELAY	; wait
	call 	TURN_PORT_OFF
	btfsc	portHeader,0x5	; skip if second header bit is zero
	call	TURN_PORT_ON
	call 	TRANSFER_DELAY	; wait
	call 	TURN_PORT_OFF
	btfsc	portHeader,0x6	; skip if third header bit is zero
	call	TURN_PORT_ON
	call 	TRANSFER_DELAY	; wait
	call 	TURN_PORT_OFF	
	btfsc	portHeader,0x7	; skip if fourth header bit is zero
	call	TURN_PORT_ON
	call 	TRANSFER_DELAY	; wait
	call 	TURN_PORT_OFF
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
	;goto TRANSFER_HEADER	;tmp
return

;***********************  
;	CHECK_FOR_SIGNAL
; 	Check if data is being transfered. The first signal is allways a "wake up" signal
;***********************
CHECK_FOR_SIGNAL:

	movfw	GPIO
	btfsc	GPIO, _portData
	call	START_RECEIVE
	return

;***********************  
;	START_RECEIVE
; 	Handles incoming dataSignal
;***********************
START_RECEIVE:

	clrf	portCounter		;clear the portCounter
	clrf	portData
	clrf	portSelect
;	clrf	portHeaderSize
	clrf	portHeader

	call 	PAUSE_HALF_SIGNAL	; pause to get "of sync"
	call 	PAUSE_SIGNAL		; pause and wait for the first incomming data bits
	call	META_TRANSFER		; fetches port select and header size
	call	HEADER_TRANSFER		; fetches header data
	call	DATA_TRANSFER		; fetches data

	return

;***********************  
;	PORT_SELECT_TRANSFER
; 	Initialize serialized data transfer to select servo
;	Only one output port may be selected
;***********************

META_TRANSFER:

	movlw	b'00000000'
	movwf	portSelect	

	btfsc	GPIO,_portData	; if signal0 is high
	bsf		portSelect,0	; use port0 as output
	call PAUSE_SIGNAL		; wait for next signal

	btfsc	GPIO,_portData	; if signal1 is high
	bsf		portSelect,1	; use port1 as output
	call PAUSE_SIGNAL		; wait for next signal

	return


;***********************  
;	HEADER_TRANSFER
; 	Initialize serialized data transfer containing interval
;***********************
HEADER_TRANSFER:
	movlw	b'00000000'
	movwf	portCounter
	bsf		portCounter,_headerBit	;init the dataCounter to the header bit
HEADER_TRANSFER_LOOP:
	movf	portCounter,w	;move counter to w
	btfsc	GPIO,_portData	;if signal is received
	iorwf	portHeader,1		;OR the portCounter to portData
	bcf		STATUS,0		;clear the carry-bit
	rlf		portCounter,1		;rotate portCounter left
	btfsc	STATUS,0x0			; if carry is set: finish transfer (full rotation)
	goto 	HEADER_TRANSFER_FINISHED
	call 	PAUSE_SIGNAL			; wait for next signal
	goto	HEADER_TRANSFER_LOOP			; fetch next bit	
HEADER_TRANSFER_FINISHED:
	call 	PAUSE_SIGNAL			; wait for next signal
	return



;***********************  
;	DATA_TRANSFER
; 	Initialize serialized data transfer containing interval
;***********************
DATA_TRANSFER:
	movlw	b'00000000'
	movwf	portCounter
	movwf	portData
	bsf		portCounter,0x0	;init the dataCounter to the first bit
DATA_TRANSFER_LOOP:
	movf	portCounter,w	;move counter to w
	btfsc	GPIO,_portData	;if signal is received
	iorwf	portData,1		;OR the portCounter to portData
	bcf		STATUS,0		;clear the carry-bit
	rlf		portCounter,1	;rotate portCounter left
	
	btfsc	STATUS,0x0			; if carry is set: finish transfer (full rotation)
	goto 	DATA_TRANSFER_FINISHED
	call 	PAUSE_SIGNAL			; wait for next signal
	goto	DATA_TRANSFER_LOOP			; fetch next bit	
DATA_TRANSFER_FINISHED:
	call 	PAUSE_SIGNAL			; wait for the phidget to set the status port
; not needed for the arduino. arduino is safe:	btfsc	GPIO,_portStatus	 ; check the status port is set (indicating that the transfer was not corrupt)
	call	TRANSFER			 ; if package was not corrupt: transfer data according to port settings	

	return

;***********************  
;	PAUSE_SIGNAL
; 	Pauses for the time required to receive a new signal
;	Optimized for a 4 MHz HC49 low profile crystal and 
;	2 x 22 pF ceramic capacitors
;***********************
PAUSE_HALF_SIGNAL:
	call PAUSE_10MS
	return

PAUSE_SIGNAL:
	call PAUSE_10MS
	call PAUSE_10MS
	return

;***********************  
;	PAUSE_10MS
; 	Pauses for 10 milliseconds. 
;***********************
PAUSE_10MS:

	movlw	d'10'
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
	btfsc	portSelect, 0x0
	bsf		GPIO, _portOut0
	btfsc	portSelect, 0x1
	bsf		GPIO, _portOut1

	
;	movfw	portSelect
;	iorwf	GPIO,1
;	bsf		GPIO,0x0
;	movwf	GPIO
return
TURN_PORT_OFF:
	movlw	0x0
	movwf	GPIO
;	movfw	portSelect
	
;	bcf		GPIO,0x0
;	bcf		GPIO,0x1
return


LAMPTESTET:
	movlw	0x2
	xorwf	GPIO,1
return
end
