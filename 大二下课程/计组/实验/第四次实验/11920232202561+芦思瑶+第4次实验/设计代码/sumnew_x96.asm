.MODEL SMALL
.STACK 100H

.DATA
    msg1 DB 'Sum= ', '$'
    sum_result DW ?
    str_sum DB 10 DUP('$') ; 用于存储转换后的字符串

.CODE
MAIN PROC
    MOV AX, @DATA
    MOV DS, AX

    ; 这里模拟累加和计算
    MOV CX, 10 ; 假设 n = 10
    MOV AX, 0
    MOV BX, 1
SUM_LOOP:
    ADD AX, BX
    INC BX
    CMP BX, CX
    JLE SUM_LOOP

    MOV sum_result, AX

    ; 转换累加和为字符串
    MOV AX, sum_result
    MOV SI, OFFSET str_sum
    CALL HEX_TO_DEC ; 自定义的十六进制转十进制字符串函数

    ; 输出 "Sum= "
    MOV AH, 09H
    LEA DX, msg1
    INT 21H

    ; 输出累加和字符串
    MOV AH, 09H
    LEA DX, str_sum
    INT 21H

    ; 退出程序
    MOV AH, 4CH
    INT 21H
MAIN ENDP

; 十六进制转十进制字符串函数
HEX_TO_DEC PROC
    MOV CX, 0
    MOV BX, 10
CONVERT_LOOP:
    XOR DX, DX
    DIV BX
    ADD DL, '0'
    PUSH DX
    INC CX
    CMP AX, 0
    JNE CONVERT_LOOP

    MOV SI, 0
POP_LOOP:
    POP [SI + str_sum]
    INC SI
    LOOP POP_LOOP
    RET
HEX_TO_DEC ENDP

END MAIN