; gpasm -dos -w1 ctest.asm -o ctest.hex; cat ctest.lst | grep Error; sudo pk2cmd -P PIC12f629 -M -F ctest.hex -Y -T
	#include    <12f629.inc>

	__config (_INTRC_OSC_NOCLKOUT & _WDT_OFF & _PWRTE_OFF & _MCLRE_OFF & _CP_OFF )

#DEFINE		_portData	0x4				; The input port

cblock 0x20		; Start of memory block

	tmp
endc

    org 0x0

INIT:
	clrw
	clrf	GPIO
	clrf	TRISIO
	bsf     STATUS,RP0       	; select Register Page 1
 	movlw	b'0'
	movwf	TRISIO
	bsf		TRISIO,_portData	; set _portData as input
;	bsf		WPU,_portData		; set _portData as weak pul-up
	bcf     STATUS,RP0       	; back to Register Page 0


MAIN:

call	TURN_PORT_ON
call	TURN_PORT_OFF

goto MAIN


TURN_PORT_ON:
	;TODO: select port
	bsf		GPIO,0x0
return
TURN_PORT_OFF:
	;TODO: select port
	bcf		GPIO,0x0
return

end
