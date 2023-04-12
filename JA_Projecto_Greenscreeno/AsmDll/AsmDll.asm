.data
greenMask dd -0.5, 1.0, -0.5, 0.0 

.code
processPictureAsm proc

start:
        cmp rdx, r8 
        je ending
        pmovzxbd xmm0, [rcx + rdx]
        cvtdq2ps xmm0, xmm0
        movups xmm1, [greenMask] 
        dpps xmm0, xmm1, 241
        comiss xmm0, xmm3
        ja changeToZero
        add rdx, 4
        jmp start

changeToZero:
        mov BYTE PTR [rcx + rdx + 3], 0
        add rdx, 4
        jmp start

ending: 
        ret

processPictureAsm endp
end