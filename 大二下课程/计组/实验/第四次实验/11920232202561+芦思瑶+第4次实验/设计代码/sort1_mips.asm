.data
    array: .word 8, 1, 5, 2, 7, 9, 6, 4, 3, 10

.text
.globl main
main:
    li $s0, 0
    li $s1, 9

outer_loop:
    move $t0, $s0
inner_loop:
    sll $t1, $t0, 2
    lw $t2, 0($t1)
    addi $t3, $t0, 1
    sll $t4, $t3, 2
    lw $t5, 0($t4)
    slt $t6, $t5, $t2
    beq $t6, $0, skip
    sw $t5, 0($t1)
    sw $t2, 0($t4)
skip:
    addi $t0, $t0, 1
    slt $t7, $t0, $s1
    beq $t7, $1, inner_loop
    addi $s0, $s0, 1
    slt $t8, $s0, $s1
    beq $t8, $1, outer_loop

    li $v0, 10
    syscall